using BusinessObjects.Common;
using DTOs.ParentMedicationDeliveryDetail.Respond;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Respond;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interfaces;
using Services.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class ParentMedicationDeliveryService : BaseService<ParentMedicationDelivery, Guid>, IParentMedicationDeliveryService
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ISchoolHealthEmailService _emailService;
        private readonly ILogger<ParentMedicationDeliveryService> _logger;

        public ParentMedicationDeliveryService(
            IGenericRepository<ParentMedicationDelivery, Guid> parentMedicationDeliveryRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            SchoolHealthManagerDbContext dbContext,
            ICurrentTime currentTime,
            ISchoolHealthEmailService emailService,
            ILogger<ParentMedicationDeliveryService> logger
            )
        :
            base(parentMedicationDeliveryRepository, currentUserService, unitOfWork, currentTime)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResult<ParentMedicationDeliveryResponseDTO>> CreateDeliveryAsync(CreateParentMedicationDeliveryRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Bắt đầu tạo phiếu giao thuốc cho StudentId: {StudentId}", request.StudentId);

                // Validation
                if (request == null)
                {
                    _logger.LogError("Request không được null");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentNullException(nameof(request)));
                }

                if (request.StudentId == Guid.Empty)
                {
                    _logger.LogError("StudentId không hợp lệ");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("StudentId không hợp lệ"));
                }

                if (request.Medications == null || !request.Medications.Any())
                {
                    _logger.LogError("Danh sách thuốc không được để trống");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("Danh sách thuốc không được để trống"));
                }

                var currentTime = _currentTime.GetVietnamTime();
                if (currentTime.Hour < 7 || currentTime.Hour > 23)
                {
                    _logger.LogError("Thời gian hiện tại không hợp lệ. Chỉ cho phép tạo phiếu giao thuốc trong giờ làm việc (08:00 - 23:00)");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("Chỉ cho phép tạo phiếu giao thuốc trong giờ làm việc (08:00 - 17:00)"));
                }

                // Validate từng medication
                foreach (var medication in request.Medications)
                {
                    if (string.IsNullOrWhiteSpace(medication.MedicationName))
                    {
                        _logger.LogError("Tên thuốc không được để trống");
                        return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("Tên thuốc không được để trống"));
                    }

                    if (medication.QuantityDelivered <= 0)
                    {
                        _logger.LogError("Số lượng thuốc phải lớn hơn 0. Thuốc: {MedicationName}", medication.MedicationName);
                        return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException($"Số lượng thuốc {medication.MedicationName} phải lớn hơn 0"));
                    }

                    if (medication.DailySchedule == null || !medication.DailySchedule.Any())
                    {
                        _logger.LogError("Lịch uống thuốc không được để trống. Thuốc: {MedicationName}", medication.MedicationName);
                        return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException($"Lịch uống thuốc {medication.MedicationName} không được để trống"));
                    }

                    // Validate daily schedule
                    foreach (var schedule in medication.DailySchedule)
                    {
                        if (schedule.Dosage <= 0)
                        {
                            _logger.LogError("Liều lượng phải lớn hơn 0. Thuốc: {MedicationName}, Thời gian: {Time}", medication.MedicationName, schedule.Time);
                            return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException($"Liều lượng thuốc {medication.MedicationName} lúc {schedule.Time} phải lớn hơn 0"));
                        }

                        if (schedule.Dosage > medication.QuantityDelivered)
                        {
                            _logger.LogError("Liều lượng không được lớn hơn số lượng thuốc. Thuốc: {MedicationName}, Thời gian: {Time}, Liều lượng: {Dosage}, Số lượng: {Quantity}", 
                                medication.MedicationName, schedule.Time, schedule.Dosage, medication.QuantityDelivered);
                            return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException($"Liều lượng thuốc {medication.MedicationName} lúc {schedule.Time} không được lớn hơn số lượng"));
                        }

                        if (IsWithinWorkingHours(schedule.Time) == false)
                        {
                            _logger.LogError("Thời gian uống thuốc phải trong giờ làm việc. Thuốc: {MedicationName}, Thời gian: {Time}", medication.MedicationName, schedule.Time);
                            return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException($"Thời gian uống thuốc {medication.MedicationName} lúc {schedule.Time} phải trong giờ làm việc"));
                        }
                    }

                    // Kiểm tra tổng liều lượng mỗi ngày
                    var totalDosagePerDay = medication.DailySchedule.Sum(s => s.Dosage);
                    if (totalDosagePerDay <= 0)
                    {
                        _logger.LogError("Tổng liều lượng mỗi ngày phải lớn hơn 0. Thuốc: {MedicationName}", medication.MedicationName);
                        return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException($"Tổng liều lượng mỗi ngày của thuốc {medication.MedicationName} phải lớn hơn 0"));
                    }

                    // Kiểm tra số ngày có thể uống
                    var numberOfDays = medication.QuantityDelivered / totalDosagePerDay;
                    if (numberOfDays <= 0)
                    {
                        _logger.LogError("Số lượng thuốc không đủ cho 1 ngày. Thuốc: {MedicationName}, Số lượng: {Quantity}, Liều/ngày: {DosagePerDay}", 
                            medication.MedicationName, medication.QuantityDelivered, totalDosagePerDay);
                        return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException($"Số lượng thuốc {medication.MedicationName} không đủ cho 1 ngày"));
                    }

                    _logger.LogInformation("Thuốc {MedicationName}: {Quantity} viên, {DosagePerDay} viên/ngày, {NumberOfDays} ngày", 
                        medication.MedicationName, medication.QuantityDelivered, totalDosagePerDay, numberOfDays);
                }

                // Kiểm tra student có tồn tại không
                var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId);
                if (student == null)
                {
                    _logger.LogError("Không tìm thấy học sinh với ID: {StudentId}", request.StudentId);
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("Không tìm thấy học sinh"));
                }

                var currentUserId = _currentUserService.GetUserId();
                if (currentUserId == null || currentUserId == Guid.Empty)
                {
                    _logger.LogError("Không thể xác định người dùng hiện tại");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new UnauthorizedAccessException("Không thể xác định người dùng hiện tại"));
                }

                var delivery = new ParentMedicationDelivery
                {
                    Id = Guid.NewGuid(),
                    StudentId = request.StudentId,
                    ParentId = currentUserId.Value,
                    Notes = request.Notes,
                    DeliveredAt = _currentTime.GetVietnamTime(),
                    Status = StatusMedicationDelivery.Pending,
                    Details = request.Medications.Select(m => new ParentMedicationDeliveryDetail
                    {
                        Id = Guid.NewGuid(),
                        MedicationName = m.MedicationName,
                        TotalQuantity = m.QuantityDelivered,
                        QuantityUsed = 0, // Khởi tạo số lượng đã sử dụng = 0
                        QuantityRemaining = m.QuantityDelivered, // Khởi tạo số lượng còn lại = tổng số lượng
                        DosageInstruction = m.DosageInstruction,
                        MedicationSchedules = m.DailySchedule.Select(schedule => new MedicationSchedule
                        {
                            Id = Guid.NewGuid(),
                            Time = schedule.Time,
                            Dosage = schedule.Dosage,
                            Note = schedule.Note
                        }).ToList()
                    }).ToList()
                };

                await _unitOfWork.ParentMedicationDeliveryRepository.AddAsync(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Tạo phiếu giao thuốc thành công. DeliveryId: {DeliveryId}, StudentId: {StudentId}, Số loại thuốc: {MedicationCount}", 
                    delivery.Id, delivery.StudentId, delivery.Details.Count);

                var response = MapToResponseDTO(delivery);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Success(response, "Tạo phiếu giao thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiếu giao thuốc cho StudentId: {StudentId}", request?.StudentId);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new Exception("Lỗi khi tạo đơn thuốc phụ huynh: " + ex.Message));
            }
        }

        public async Task<ApiResult<ParentMedicationDeliveryResponseDTO>> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Lấy phiếu giao thuốc theo ID: {Id}", id);

                if (id == Guid.Empty)
                {
                    _logger.LogError("ID không hợp lệ");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("ID không hợp lệ"));
                }

                var delivery = await _unitOfWork.ParentMedicationDeliveryRepository
                    .GetQueryable()
                    .Include(d => d.Student)
                    .Include(d => d.Details)
                        .ThenInclude(d => d.MedicationSchedules)
                    .OrderByDescending(d => d.DeliveredAt)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (delivery == null)
                {
                    _logger.LogWarning("Không tìm thấy phiếu giao thuốc với ID: {Id}", id);
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new Exception("Không tìm thấy phiếu giao thuốc."));
                }

                _logger.LogInformation("Lấy phiếu giao thuốc thành công. ID: {Id}, StudentId: {StudentId}", id, delivery.StudentId);

                var response = MapToResponseDTO(delivery);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Success(response, "Lấy phiếu giao thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy phiếu giao thuốc theo ID: {Id}", id);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<ParentMedicationDeliveryResponseDTO>>> GetByStudentIdAsync(Guid studentId)
        {
            try
            {
                _logger.LogInformation("Lấy danh sách phiếu giao thuốc theo StudentId: {StudentId}", studentId);

                if (studentId == Guid.Empty)
                {
                    _logger.LogError("StudentId không hợp lệ");
                    return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Failure(new ArgumentException("StudentId không hợp lệ"));
                }

                var deliveries = await _unitOfWork.ParentMedicationDeliveryRepository
                    .GetQueryable()
                    .Include(d => d.Student)
                    .Include(d => d.Details)
                        .ThenInclude(d => d.MedicationSchedules)
                    .OrderByDescending(d => d.DeliveredAt)
                    .Where(d => d.StudentId == studentId)
                    .ToListAsync();

                _logger.LogInformation("Lấy danh sách phiếu giao thuốc thành công. StudentId: {StudentId}, Số lượng: {Count}", studentId, deliveries.Count);

                var response = deliveries.Select(delivery => MapToResponseDTO(delivery)).ToList();
                return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Success(response, "Lấy danh sách phiếu giao thuốc theo học sinh thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu giao thuốc theo StudentId: {StudentId}", studentId);
                return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<ParentMedicationDeliveryResponseDTO>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Lấy tất cả phiếu giao thuốc");

                var deliveries = await _unitOfWork.ParentMedicationDeliveryRepository
                    .GetQueryable()
                    .Include(d => d.Student)
                    .Include(d => d.Details)
                        .ThenInclude(d => d.MedicationSchedules)
                    .OrderByDescending(d => d.DeliveredAt)
                    .ToListAsync();

                _logger.LogInformation("Lấy tất cả phiếu giao thuốc thành công. Số lượng: {Count}", deliveries.Count);

                var response = deliveries.Select(delivery => MapToResponseDTO(delivery)).ToList();
                return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Success(response, "Lấy tất cả phiếu giao thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả phiếu giao thuốc");
                return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<ParentMedicationDeliveryResponseDTO>>> GetAllForCurrentParentAsync()
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                if (currentUserId == null || currentUserId == Guid.Empty)
                {
                    _logger.LogError("Không thể xác định người dùng hiện tại");
                    return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Failure(
                        new UnauthorizedAccessException("Không thể xác định người dùng hiện tại"));
                }

                var parent = await _dbContext.Parents
                    .Include(p => p.Students)
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                if (parent == null)
                {
                    _logger.LogWarning("Không tìm thấy phụ huynh với UserId: {UserId}", currentUserId);
                    return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Failure(
                        new Exception("Phụ huynh không tồn tại hoặc không có quyền truy cập"));
                }

                _logger.LogInformation("Lấy danh sách giao thuốc cho phụ huynh {ParentId}", parent.UserId);

                var deliveries = await _unitOfWork.ParentMedicationDeliveryRepository
                    .GetQueryable()
                    .Include(d => d.Student)
                    .Include(d => d.Details)
                        .ThenInclude(detail => detail.MedicationSchedules)
                    .Where(d => d.ParentId == parent.UserId)
                    .OrderByDescending(d => d.DeliveredAt)
                    .ToListAsync();

                _logger.LogInformation("Lấy thành công {Count} phiếu giao thuốc cho phụ huynh", deliveries.Count);

                var response = deliveries.Select(delivery => MapToResponseDTO(delivery)).ToList();
                return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Success(response, "Lấy danh sách giao thuốc thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao thuốc cho phụ huynh");
                return ApiResult<List<ParentMedicationDeliveryResponseDTO>>.Failure(
                    new Exception("Lỗi khi lấy danh sách giao thuốc: " + ex.Message));
            }
        }


        public async Task<ApiResult<ParentMedicationDeliveryResponseDTO>> UpdateStatusAsync(Guid deliveryId, StatusMedicationDelivery status)
        {
            try
            {
                _logger.LogInformation("Cập nhật trạng thái phiếu giao thuốc. DeliveryId: {DeliveryId}, Status: {Status}", deliveryId, status);

                if (deliveryId == Guid.Empty)
                {
                    _logger.LogError("DeliveryId không hợp lệ");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("DeliveryId không hợp lệ"));
                }

                var currentid = _currentUserService.GetUserId();    
                if (currentid == null || currentid == Guid.Empty)
                {
                    _logger.LogError("Không thể xác định người dùng hiện tại");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new UnauthorizedAccessException("Không thể xác định người dùng hiện tại"));
                }
                var delivery = await _unitOfWork.ParentMedicationDeliveryRepository
                    .GetQueryable()
                    .Include(d => d.Details)
                        .ThenInclude(d => d.MedicationSchedules)
                    .FirstOrDefaultAsync(d => d.Id == deliveryId);

                if (delivery == null)
                {
                    _logger.LogWarning("Không tìm thấy phiếu giao thuốc với ID: {DeliveryId}", deliveryId);
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new Exception("Không tìm thấy phiếu giao thuốc."));
                }

                // Kiểm tra trạng thái hiện tại
                if (delivery.Status == StatusMedicationDelivery.Confirmed && status == StatusMedicationDelivery.Confirmed)
                {
                    _logger.LogWarning("Phiếu giao thuốc đã được xác nhận trước đó. DeliveryId: {DeliveryId}", deliveryId);
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new InvalidOperationException("Phiếu giao thuốc đã được xác nhận trước đó"));
                }

                delivery.Status = status;
                
                // Nếu status là Confirmed, tự động tạo MedicationUsageRecord cho từng ngày
                if (status == StatusMedicationDelivery.Confirmed)
                {
                    _logger.LogInformation("Bắt đầu tạo MedicationUsageRecord cho phiếu giao thuốc. DeliveryId: {DeliveryId}", deliveryId);
                    delivery.ReceivedBy = _currentUserService.GetUserId() ?? Guid.Empty;
                    await CreateMedicationUsageRecordsAsync(delivery);
                    _logger.LogInformation("Tạo MedicationUsageRecord thành công cho phiếu giao thuốc. DeliveryId: {DeliveryId}", deliveryId);
                }

                await _unitOfWork.ParentMedicationDeliveryRepository.UpdateAsync(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cập nhật trạng thái phiếu giao thuốc thành công. DeliveryId: {DeliveryId}, Status: {Status}", deliveryId, status);

                var response = MapToResponseDTO(delivery);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Success(response, "Cập nhật trạng thái phiếu giao thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái phiếu giao thuốc. DeliveryId: {DeliveryId}, Status: {Status}", deliveryId, status);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<ParentMedicationDeliveryResponseDTO>> DeleteAsync(Guid deliveryId)
        {
            try
            {
                _logger.LogInformation("Xóa phiếu giao thuốc. DeliveryId: {DeliveryId}", deliveryId);

                if (deliveryId == Guid.Empty)
                {
                    _logger.LogError("DeliveryId không hợp lệ");
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new ArgumentException("DeliveryId không hợp lệ"));
                }

                var delivery = await _unitOfWork.ParentMedicationDeliveryRepository
                    .GetQueryable()
                    .Include(d => d.Details)
                        .ThenInclude(d => d.MedicationSchedules)
                    .FirstOrDefaultAsync(d => d.Id == deliveryId);

                if (delivery == null)
                {
                    _logger.LogWarning("Không tìm thấy phiếu giao thuốc với ID: {DeliveryId}", deliveryId);
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new Exception("Không tìm thấy phiếu giao thuốc."));
                }

                // Kiểm tra xem có thể xóa không
                if (delivery.Status == StatusMedicationDelivery.Confirmed)
                {
                    _logger.LogWarning("Không thể xóa phiếu giao thuốc đã được xác nhận. DeliveryId: {DeliveryId}", deliveryId);
                    return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(new InvalidOperationException("Không thể xóa phiếu giao thuốc đã được xác nhận"));
                }

                await _unitOfWork.ParentMedicationDeliveryRepository.DeleteAsync(deliveryId);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Xóa phiếu giao thuốc thành công. DeliveryId: {DeliveryId}", deliveryId);

                var response = MapToResponseDTO(delivery);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Success(response, "Xóa phiếu giao thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phiếu giao thuốc. DeliveryId: {DeliveryId}", deliveryId);
                return ApiResult<ParentMedicationDeliveryResponseDTO>.Failure(ex);
            }
        }

        // Helper method để tạo MedicationUsageRecord tự động
        private async Task CreateMedicationUsageRecordsAsync(ParentMedicationDelivery delivery)
        {
            try
            {
                var currentDate = _currentTime.GetVietnamTime().Date;
                var usageRecords = new List<MedicationUsageRecord>();

                _logger.LogInformation("Bắt đầu tạo MedicationUsageRecord. DeliveryId: {DeliveryId}, Số loại thuốc: {DetailCount}", 
                    delivery.Id, delivery.Details.Count);

                foreach (var detail in delivery.Details)
                {
                    // Tính tổng số viên uống mỗi ngày
                    var totalDosagePerDay = detail.MedicationSchedules.Sum(s => s.Dosage);
                    
                    // Tính số ngày có thể uống
                    var numberOfDays = detail.TotalQuantity / totalDosagePerDay;
                    
                    // Nếu không chia hết, làm tròn xuống
                    if (detail.TotalQuantity % totalDosagePerDay != 0)
                    {
                        numberOfDays = (int)Math.Floor((double)detail.TotalQuantity / totalDosagePerDay);
                    }

                    _logger.LogInformation("Thuốc {MedicationName}: {TotalQuantity} viên, {TotalDosagePerDay} viên/ngày, {NumberOfDays} ngày", 
                        detail.MedicationName, detail.TotalQuantity, totalDosagePerDay, numberOfDays);

                    // Tạo record cho từng ngày
                    for (int day = 0; day < numberOfDays; day++)
                    {
                        var currentDay = currentDate.AddDays(day);
                        
                        foreach (var schedule in detail.MedicationSchedules)
                        {
                            var scheduledDateTime = currentDay.Add(schedule.Time);
                            
                            usageRecords.Add(new MedicationUsageRecord
                            {
                                Id = Guid.NewGuid(),
                                DeliveryDetailId = detail.Id,
                                MedicationScheduleId = schedule.Id,
                                ScheduledAt = scheduledDateTime,
                                IsTaken = false,
                                Note = null
                            });
                        }
                    }
                }

                await _dbContext.MedicationUsageRecords.AddRangeAsync(usageRecords);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Tạo MedicationUsageRecord thành công. DeliveryId: {DeliveryId}, Số records: {RecordCount}", 
                    delivery.Id, usageRecords.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo MedicationUsageRecord. DeliveryId: {DeliveryId}", delivery.Id);
                throw;
            }
        }

        // Helper method để map từ entity sang DTO
        private ParentMedicationDeliveryResponseDTO MapToResponseDTO(ParentMedicationDelivery delivery)
        {
            return new ParentMedicationDeliveryResponseDTO
            {
                Id = delivery.Id,
                StudentId = delivery.StudentId,
                StudentName = delivery.Student?.FirstName+" "+delivery.Student?.LastName ?? string.Empty,
                ParentId = delivery.ParentId,
                ReceivedBy = delivery.ReceivedBy ?? Guid.Empty,
                Notes = delivery.Notes ?? string.Empty,
                DeliveredAt = delivery.DeliveredAt,
                Status = delivery.Status,
                Medications = delivery.Details.Select(md => new ParentMedicationDeliveryDetailResponseDTO
                {
                    Id = md.Id,
                    MedicationName = md.MedicationName,
                    TotalQuantity = md.TotalQuantity,
                    QuantityUsed = md.QuantityUsed,
                    QuantityRemaining = md.QuantityRemaining,
                    DosageInstruction = md.DosageInstruction,
                    ReturnedQuantity = md.ReturnedQuantity,
                    ReturnedAt = md.ReturnedAt,
                    DailySchedule = md.MedicationSchedules?.Select(ms => new MedicationScheduleResponseDTO
                    {
                        Id = ms.Id,
                        Time = ms.Time,
                        Dosage = ms.Dosage,
                        Note = ms.Note
                    }).ToList() ?? new List<MedicationScheduleResponseDTO>()
                }).ToList()
            };
        }

    }
}
