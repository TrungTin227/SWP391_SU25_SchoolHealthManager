using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;
using DTOs.MedicationUsageRecord.Request;
using DTOs.MedicationUsageRecord.Respond;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interfaces;
using Services.Commons;
using Services.Interfaces;

namespace Services.Implementations
{
    public class MedicationUsageRecordService : BaseService<MedicationUsageRecord, Guid>, IMedicationUsageRecordService
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ILogger<MedicationUsageRecordService> _logger;

        public MedicationUsageRecordService(
            IGenericRepository<MedicationUsageRecord, Guid> medicationUsageRecordRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            SchoolHealthManagerDbContext dbContext,
            ICurrentTime currentTime,
            ILogger<MedicationUsageRecordService> logger)
            : base(medicationUsageRecordRepository, currentUserService, unitOfWork, currentTime)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetByDeliveryDetailIdAsync(Guid deliveryDetailId)
        {
            try
            {
                _logger.LogInformation("Lấy danh sách record uống thuốc theo DeliveryDetailId: {DeliveryDetailId}", deliveryDetailId);

                if (deliveryDetailId == Guid.Empty)
                {
                    _logger.LogError("DeliveryDetailId không hợp lệ");
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException("DeliveryDetailId không hợp lệ"));
                }

                // Kiểm tra delivery detail có tồn tại không
                var deliveryDetail = await _dbContext.ParentMedicationDeliveryDetails
                    .FirstOrDefaultAsync(d => d.Id == deliveryDetailId);
                if (deliveryDetail == null)
                {
                    _logger.LogWarning("Không tìm thấy delivery detail với ID: {DeliveryDetailId}", deliveryDetailId);
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException("Không tìm thấy delivery detail"));
                }

                var records = await _dbContext.MedicationUsageRecords
                    .Include(r => r.DeliveryDetail)
                        .ThenInclude(d => d.ParentMedicationDelivery)
                            .ThenInclude(p => p.Student)
                    .Include(r => r.MedicationSchedule)
                    .Include(r => r.Nurse)
                    .Where(r => r.DeliveryDetailId == deliveryDetailId)
                    .OrderBy(r => r.ScheduledAt)
                    .ToListAsync();

                _logger.LogInformation("Lấy danh sách record uống thuốc thành công. DeliveryDetailId: {DeliveryDetailId}, Số lượng: {Count}", 
                    deliveryDetailId, records.Count);

                var response = records.Select(MapToResponseDTO).ToList();
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Success(response, "Lấy danh sách record uống thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách record uống thuốc theo DeliveryDetailId: {DeliveryDetailId}", deliveryDetailId);
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetByStudentIdAsync(Guid studentId)
        {
            try
            {
                _logger.LogInformation("Lấy danh sách record uống thuốc theo StudentId: {StudentId}", studentId);

                if (studentId == Guid.Empty)
                {
                    _logger.LogError("StudentId không hợp lệ");
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException("StudentId không hợp lệ"));
                }

                // Kiểm tra student có tồn tại không
                var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == studentId);
                if (student == null)
                {
                    _logger.LogWarning("Không tìm thấy học sinh với ID: {StudentId}", studentId);
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException("Không tìm thấy học sinh"));
                }

                var records = await _dbContext.MedicationUsageRecords
                    .Include(r => r.DeliveryDetail)
                        .ThenInclude(d => d.ParentMedicationDelivery)
                            .ThenInclude(p => p.Student)
                    .Include(r => r.MedicationSchedule)
                    .Include(r => r.Nurse)
                    .Where(r => r.DeliveryDetail.ParentMedicationDelivery.StudentId == studentId)
                    .OrderBy(r => r.ScheduledAt)
                    .ToListAsync();

                _logger.LogInformation("Lấy danh sách record uống thuốc theo học sinh thành công. StudentId: {StudentId}, Số lượng: {Count}", 
                    studentId, records.Count);

                var response = records.Select(MapToResponseDTO).ToList();
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Success(response, "Lấy danh sách record uống thuốc theo học sinh thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách record uống thuốc theo StudentId: {StudentId}", studentId);
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetByDateAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("Lấy danh sách record uống thuốc theo ngày: {Date}", date.ToString("yyyy-MM-dd"));

                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                var records = await _dbContext.MedicationUsageRecords
                    .Include(r => r.DeliveryDetail)
                        .ThenInclude(d => d.ParentMedicationDelivery)
                            .ThenInclude(p => p.Student)
                    .Include(r => r.MedicationSchedule)
                    .Include(r => r.Nurse)
                    .Where(r => r.ScheduledAt >= startDate && r.ScheduledAt < endDate)
                    .OrderBy(r => r.ScheduledAt)
                    .ToListAsync();

                _logger.LogInformation("Lấy danh sách record uống thuốc theo ngày thành công. Ngày: {Date}, Số lượng: {Count}", 
                    date.ToString("yyyy-MM-dd"), records.Count);

                var response = records.Select(MapToResponseDTO).ToList();
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Success(response, "Lấy danh sách record uống thuốc theo ngày thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách record uống thuốc theo ngày: {Date}", date.ToString("yyyy-MM-dd"));
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicationUsageRecordResponseDTO>> UpdateTakenStatusAsync(UpdateMedicationUsageRecordDTO request)
        {
            try
            {
                _logger.LogInformation("Cập nhật trạng thái uống thuốc. RecordId: {RecordId}, IsTaken: {IsTaken}", request.Id, request.IsTaken);

                // Validation
                if (request == null)
                {
                    _logger.LogError("Request không được null");
                    return ApiResult<MedicationUsageRecordResponseDTO>.Failure(new ArgumentNullException(nameof(request)));
                }

                if (request.Id == Guid.Empty)
                {
                    _logger.LogError("RecordId không hợp lệ");
                    return ApiResult<MedicationUsageRecordResponseDTO>.Failure(new ArgumentException("RecordId không hợp lệ"));
                }

                var record = await _dbContext.MedicationUsageRecords
                    .Include(r => r.DeliveryDetail)
                        .ThenInclude(d => d.ParentMedicationDelivery)
                            .ThenInclude(p => p.Student).Include(r => r.MedicationSchedule)
                    .Include(r => r.Nurse)
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (record == null)
                {
                    _logger.LogWarning("Không tìm thấy record uống thuốc với ID: {RecordId}", request.Id);
                    return ApiResult<MedicationUsageRecordResponseDTO>.Failure(new Exception("Không tìm thấy record uống thuốc."));
                }

                // Kiểm tra xem record có thể cập nhật không
                var currentTime = _currentTime.GetVietnamTime();
                if (record.ScheduledAt > currentTime)
                {
                    _logger.LogWarning("Không thể cập nhật record chưa đến giờ uống. RecordId: {RecordId}, ScheduledAt: {ScheduledAt}, CurrentTime: {CurrentTime}", 
                        request.Id, record.ScheduledAt, currentTime);
                    return ApiResult<MedicationUsageRecordResponseDTO>.Failure(new InvalidOperationException("Không thể cập nhật record chưa đến giờ uống"));
                }

                // Kiểm tra xem record đã được cập nhật chưa
                //if (record.IsTaken == request.IsTaken)
                //{
                //    _logger.LogWarning("Record đã có trạng thái này. RecordId: {RecordId}, CurrentStatus: {CurrentStatus}, RequestedStatus: {RequestedStatus}", 
                //        request.Id, record.IsTaken, request.IsTaken);
                //    return ApiResult<MedicationUsageRecordResponseDTO>.Failure(new InvalidOperationException("Record đã có trạng thái này"));
                //}

                var currentUserId = _currentUserService.GetUserId();
                if (currentUserId == null || currentUserId == Guid.Empty)
                {
                    _logger.LogError("Không thể xác định người dùng hiện tại");
                    return ApiResult<MedicationUsageRecordResponseDTO>.Failure(new UnauthorizedAccessException("Không thể xác định người dùng hiện tại"));
                }

                // Cập nhật số lượng thuốc trong ParentMedicationDeliveryDetail
                if (request.IsTaken && !record.IsTaken) // Chỉ khi chuyển từ chưa uống sang đã uống
                {
                    var deliveryDetail = record.DeliveryDetail;
                    if (deliveryDetail != null)
                    {
                        // Tính số lượng thuốc đã dùng cho lần uống này
                        var dosageForThisRecord = record.MedicationSchedule?.Dosage ?? 0;

                        if (dosageForThisRecord > 0)
                        {
                            // Cập nhật số lượng thuốc đã sử dụng và còn lại
                            deliveryDetail.QuantityUsed += dosageForThisRecord;
                            deliveryDetail.QuantityRemaining = Math.Max(0, deliveryDetail.TotalQuantity - deliveryDetail.QuantityUsed);

                            _logger.LogInformation("Đã cập nhật số lượng thuốc. DeliveryDetailId: {DeliveryDetailId}, Đã dùng: {QuantityUsed}, Còn lại: {QuantityRemaining}",
                                deliveryDetail.Id, deliveryDetail.QuantityUsed, deliveryDetail.QuantityRemaining);

                            // Cập nhật delivery detail
                            await _unitOfWork.ParentMedicationDeliveryDetailRepository.UpdateAsync(deliveryDetail);
                        }
                    }
                }

                record.IsTaken = request.IsTaken;
                record.TakenAt = request.IsTaken ? currentTime : null;
                record.Note = request.Note;
                record.CheckedBy = currentUserId;

                //// Cập nhật số lượng thuốc trong ParentMedicationDeliveryDetail
                //if (request.IsTaken && !record.IsTaken) // Chỉ khi chuyển từ chưa uống sang đã uống
                //{
                //    var deliveryDetail = record.DeliveryDetail;
                //    if (deliveryDetail != null)
                //    {
                //        // Tính số lượng thuốc đã dùng cho lần uống này
                //        var dosageForThisRecord = record.MedicationSchedule?.Dosage ?? 0;
                        
                //        if (dosageForThisRecord > 0)
                //        {
                //            // Cập nhật số lượng thuốc đã sử dụng và còn lại
                //            deliveryDetail.QuantityUsed += dosageForThisRecord;
                //            deliveryDetail.QuantityRemaining = Math.Max(0, deliveryDetail.TotalQuantity - deliveryDetail.QuantityUsed);
                            
                //            _logger.LogInformation("Đã cập nhật số lượng thuốc. DeliveryDetailId: {DeliveryDetailId}, Đã dùng: {QuantityUsed}, Còn lại: {QuantityRemaining}", 
                //                deliveryDetail.Id, deliveryDetail.QuantityUsed, deliveryDetail.QuantityRemaining);
                            
                //            // Cập nhật delivery detail
                //            await _unitOfWork.ParentMedicationDeliveryDetailRepository.UpdateAsync(deliveryDetail);
                //        }
                //    }
                //}

                await _unitOfWork.MedicationUsageRecordRepository.UpdateAsync(record);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cập nhật trạng thái uống thuốc thành công. RecordId: {RecordId}, IsTaken: {IsTaken}, CheckedBy: {CheckedBy}", 
                    request.Id, request.IsTaken, currentUserId);

                var response = MapToResponseDTO(record);
                return ApiResult<MedicationUsageRecordResponseDTO>.Success(response, "Cập nhật trạng thái uống thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái uống thuốc. RecordId: {RecordId}", request?.Id);
                return ApiResult<MedicationUsageRecordResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> BulkUpdateTakenStatusAsync(List<UpdateMedicationUsageRecordDTO> requests)
        {
            try
            {
                _logger.LogInformation("Cập nhật hàng loạt trạng thái uống thuốc. Số lượng: {Count}", requests?.Count ?? 0);

                // Validation
                if (requests == null || !requests.Any())
                {
                    _logger.LogError("Danh sách request không được để trống");
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException("Danh sách request không được để trống"));
                }

                // Validate từng request
                foreach (var request in requests)
                {
                    if (request.Id == Guid.Empty)
                    {
                        _logger.LogError("RecordId không hợp lệ trong danh sách bulk update");
                        return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException("RecordId không hợp lệ"));
                    }
                }

                var recordIds = requests.Select(r => r.Id).ToList();
                var records = await _dbContext.MedicationUsageRecords
                    .Include(r => r.DeliveryDetail)
                        .ThenInclude(d => d.ParentMedicationDelivery)
                            .ThenInclude(p => p.Student).Include(r => r.MedicationSchedule)
                    .Include(r => r.Nurse)
                    .Where(r => recordIds.Contains(r.Id))
                    .ToListAsync();

                // Kiểm tra xem tất cả records có tồn tại không
                var foundRecordIds = records.Select(r => r.Id).ToHashSet();
                var missingRecordIds = recordIds.Where(id => !foundRecordIds.Contains(id)).ToList();
                if (missingRecordIds.Any())
                {
                    _logger.LogWarning("Không tìm thấy một số records: {MissingRecordIds}", string.Join(", ", missingRecordIds));
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException($"Không tìm thấy records: {string.Join(", ", missingRecordIds)}"));
                }

                var currentTime = _currentTime.GetVietnamTime();
                var currentUserId = _currentUserService.GetUserId();

                if (currentUserId == null || currentUserId == Guid.Empty)
                {
                    _logger.LogError("Không thể xác định người dùng hiện tại");
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new UnauthorizedAccessException("Không thể xác định người dùng hiện tại"));
                }

                var updatedCount = 0;
                foreach (var request in requests)
                {
                    var record = records.FirstOrDefault(r => r.Id == request.Id);
                    if (record != null)
                    {
                        // Kiểm tra xem record có thể cập nhật không
                        if (record.ScheduledAt > currentTime)
                        {
                            _logger.LogWarning("Bỏ qua record chưa đến giờ uống. RecordId: {RecordId}, ScheduledAt: {ScheduledAt}", 
                                request.Id, record.ScheduledAt);
                            continue;
                        }

                        // Kiểm tra xem record đã được cập nhật chưa
                        if (record.IsTaken == request.IsTaken)
                        {
                            _logger.LogWarning("Bỏ qua record đã có trạng thái này. RecordId: {RecordId}, Status: {Status}", 
                                request.Id, record.IsTaken);
                            continue;
                        }

                        // Cập nhật số lượng thuốc trong ParentMedicationDeliveryDetail
                        if (request.IsTaken && !record.IsTaken) // Chỉ khi chuyển từ chưa uống sang đã uống
                        {
                            var deliveryDetail = record.DeliveryDetail;
                            if (deliveryDetail != null)
                            {
                                // Tính số lượng thuốc đã dùng cho lần uống này
                                var dosageForThisRecord = record.MedicationSchedule?.Dosage ?? 0;

                                if (dosageForThisRecord > 0)
                                {
                                    // Cập nhật số lượng thuốc đã sử dụng và còn lại
                                    deliveryDetail.QuantityUsed += dosageForThisRecord;
                                    deliveryDetail.QuantityRemaining = Math.Max(0, deliveryDetail.TotalQuantity - deliveryDetail.QuantityUsed);

                                    _logger.LogInformation("Bulk update - Đã cập nhật số lượng thuốc. DeliveryDetailId: {DeliveryDetailId}, Đã dùng: {QuantityUsed}, Còn lại: {QuantityRemaining}",
                                        deliveryDetail.Id, deliveryDetail.QuantityUsed, deliveryDetail.QuantityRemaining);

                                    // Cập nhật delivery detail
                                    await _unitOfWork.ParentMedicationDeliveryDetailRepository.UpdateAsync(deliveryDetail);
                                }
                            }
                        }

                        record.IsTaken = request.IsTaken;
                        record.TakenAt = request.IsTaken ? currentTime : null;
                        record.Note = request.Note;
                        record.CheckedBy = currentUserId;

                        //// Cập nhật số lượng thuốc trong ParentMedicationDeliveryDetail
                        //if (request.IsTaken && !record.IsTaken) // Chỉ khi chuyển từ chưa uống sang đã uống
                        //{
                        //    var deliveryDetail = record.DeliveryDetail;
                        //    if (deliveryDetail != null)
                        //    {
                        //        // Tính số lượng thuốc đã dùng cho lần uống này
                        //        var dosageForThisRecord = record.MedicationSchedule?.Dosage ?? 0;
                                
                        //        if (dosageForThisRecord > 0)
                        //        {
                        //            // Cập nhật số lượng thuốc đã sử dụng và còn lại
                        //            deliveryDetail.QuantityUsed += dosageForThisRecord;
                        //            deliveryDetail.QuantityRemaining = Math.Max(0, deliveryDetail.TotalQuantity - deliveryDetail.QuantityUsed);
                                    
                        //            _logger.LogInformation("Bulk update - Đã cập nhật số lượng thuốc. DeliveryDetailId: {DeliveryDetailId}, Đã dùng: {QuantityUsed}, Còn lại: {QuantityRemaining}", 
                        //                deliveryDetail.Id, deliveryDetail.QuantityUsed, deliveryDetail.QuantityRemaining);
                                    
                        //            // Cập nhật delivery detail
                        //            await _unitOfWork.ParentMedicationDeliveryDetailRepository.UpdateAsync(deliveryDetail);
                        //        }
                        //    }
                        //}

                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await _unitOfWork.MedicationUsageRecordRepository.UpdateRangeAsync(records);
                    await _unitOfWork.SaveChangesAsync();
                }

                _logger.LogInformation("Cập nhật hàng loạt trạng thái uống thuốc thành công. Tổng: {Total}, Đã cập nhật: {UpdatedCount}", 
                    requests.Count, updatedCount);

                var response = records.Select(MapToResponseDTO).ToList();
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Success(response, $"Cập nhật hàng loạt trạng thái uống thuốc thành công! ({updatedCount}/{requests.Count} records)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hàng loạt trạng thái uống thuốc");
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetPendingRecordsAsync()
        {
            try
            {
                var today = _currentTime.GetVietnamTime().Date;
                var tomorrow = today.AddDays(1);

                var records = await _dbContext.MedicationUsageRecords
                    .Include(r => r.DeliveryDetail)
                        .ThenInclude(d => d.ParentMedicationDelivery)
                            .ThenInclude(p => p.Student)
                    .Include(r => r.MedicationSchedule)
                    .Include(r => r.Nurse)
                    .Where(r => !r.IsTaken && r.ScheduledAt >= today && r.ScheduledAt < tomorrow)
                    .OrderBy(r => r.ScheduledAt)
                    .ToListAsync();

                var response = records.Select(MapToResponseDTO).ToList();
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Success(response, "Lấy danh sách record chưa xác nhận thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách record chưa xác nhận.");
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> NurseBulkConfirmAsync(List<Guid> recordIds)
        {
            try
            {
                if (recordIds == null || !recordIds.Any())
                    return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(new ArgumentException("Danh sách recordId không được để trống."));

                var records = await _dbContext.MedicationUsageRecords
                    .Where(r => recordIds.Contains(r.Id) && !r.IsTaken)
                    .ToListAsync();

                var currentTime = _currentTime.GetVietnamTime();
                var currentUserId = _currentUserService.GetUserId();

                foreach (var record in records)
                {
                    record.IsTaken = true;
                    record.TakenAt = currentTime;
                    record.CheckedBy = currentUserId;
                }

                await _unitOfWork.MedicationUsageRecordRepository.UpdateRangeAsync(records);
                await _unitOfWork.SaveChangesAsync();

                var response = records.Select(MapToResponseDTO).ToList();
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Success(response, "Xác nhận uống thuốc thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác nhận uống thuốc hàng loạt.");
                return ApiResult<List<MedicationUsageRecordResponseDTO>>.Failure(ex);
            }
        }

        private MedicationUsageRecordResponseDTO MapToResponseDTO(MedicationUsageRecord record)
        {
            var student = record.DeliveryDetail?.ParentMedicationDelivery?.Student;
            return new MedicationUsageRecordResponseDTO
            {
                Id = record.Id,
                DeliveryDetailId = record.DeliveryDetailId,
                MedicationName = record.DeliveryDetail.MedicationName ?? string.Empty,
                dosageInstruction = record.DeliveryDetail.DosageInstruction ?? string.Empty,
                totalQuantity = record.DeliveryDetail.TotalQuantity,
                QuantityUsed = record.DeliveryDetail.QuantityUsed,
                QuantityRemaining = record.DeliveryDetail.QuantityRemaining,
                UsedAt = record.ScheduledAt,
                IsTaken = record.IsTaken,
                Note = record.Note,
                StudentId = student?.Id ?? Guid.Empty,
                StudentName = student?.FullName // or .Name, depending on your model
            };
        }
    }
}
