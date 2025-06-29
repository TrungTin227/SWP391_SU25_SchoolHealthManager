using Azure;
using BusinessObjects;
using DTOs.CheckUpRecordDTOs.Requests;
using DTOs.CheckUpRecordDTOs.Responds;
using DTOs.CounselingAppointmentDTOs.Requests;
using DTOs.CounselingAppointmentDTOs.Responds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class CheckupRecordService : BaseService<CheckupRecordService, Guid>, ICheckupRecordService
    {
        private readonly ICounselingAppointmentService _counselingAppointmentService;
        private readonly ILogger<CheckupRecordService> _logger;
        public CheckupRecordService(
            IGenericRepository<CheckupRecordService, Guid> repository
            , ICurrentUserService currentUserService, 
            IUnitOfWork unitOfWork, 
            ICurrentTime currentTime,
            ICounselingAppointmentService counselingAppointmentService,
            ILogger<CheckupRecordService> logger) : 
            base(repository, 
                currentUserService, 
                unitOfWork, 
                currentTime)
        {
            _counselingAppointmentService = counselingAppointmentService;
            _logger = logger;
        }
        public async Task<ApiResult<CheckupRecordRespondDTO>> CreateCheckupRecordAsync(CreateCheckupRecordRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra lịch khám tồn tại
                var schedule = await _unitOfWork.CheckupScheduleRepository.GetByIdAsync(request.ScheduleId);
                if (schedule == null)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Lịch khám không tồn tại!!"));
                }
                if (schedule.IsDeleted)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Lịch khám đã bị xóa!!"));
                }
                if (schedule.ParentConsentStatus != CheckupScheduleStatus.Approved)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Phụ huynh chưa đồng ý lịch khám này!!"));
                }
                var existingRecord = await _unitOfWork.CheckupRecordRepository
                                    .AnyAsync(x => x.ScheduleId == request.ScheduleId);
                if (existingRecord)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Lịch khám này đã có hồ sơ học sinh rồi!"));
                }

                
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(schedule.StudentId);
                if (student == null || student.IsDeleted)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Học sinh không tồn tại hoặc đã bị xóa!"));
                }


                // 2. Tạo CheckupRecord
                var checkupRecord = CheckupRecordMappings.MapToEntity(request, student);
                checkupRecord.Id = Guid.NewGuid(); // đảm bảo có ID trước khi dùng
                await _unitOfWork.CheckupRecordRepository.AddAsync(checkupRecord);

                // 3. Nếu cần tư vấn → tạo CounselingAppointment
                if (request.Status == CheckupRecordStatus.RequiresFollowUp &&
                    request.CounselingAppointment != null &&
                    request.CounselingAppointment.Any())
                {
                    foreach (var caDto in request.CounselingAppointment)
                    {
                        // Clone DTO để tránh reference bug
                        var appointmentDto = new CreateCounselingAppointmentRequestDTO
                        {
                            StudentId = student.Id,
                            ParentId = student.ParentUserId,
                            StaffUserId = caDto.StaffUserId,
                            AppointmentDate = caDto.AppointmentDate,
                            Duration = caDto.Duration,
                            Purpose = caDto.Purpose,
                            CheckupRecordId = checkupRecord.Id
                        };

                        _logger.LogWarning("🐛 Tạo entity với Purpose = {Purpose}", appointmentDto.Purpose);


                        var result = await _counselingAppointmentService.CreateCounselingAppointmentAsync(appointmentDto);

                        if (!result.IsSuccess)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Tạo appointment thất bại: " + result.Message));
                        }
                    }
                }
                await _unitOfWork.CommitTransactionAsync();
                var response = CheckupRecordMappings.MapToRespondDTO(checkupRecord);
                if (request.Status != CheckupRecordStatus.RequiresFollowUp && request.CounselingAppointment!= null && request.CounselingAppointment.Any()) 
                        return ApiResult<CheckupRecordRespondDTO>.Success(response, "Tạo hồ sơ kiểm tra và lịch hẹn tư vấn thất bại vì status phải là RequiresFollowUp!!");
                return ApiResult<CheckupRecordRespondDTO>.Success(response, "Tạo hồ sơ kiểm tra thành công!!");
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Tạo hồ sơ kiểm tra thất bại!! " + e.Message));
            }
        }

        public async Task<ApiResult<List<CheckupRecordRespondDTO?>>> GetAllByStaffIdAsync(Guid id)
        {
            try
            {
                var isNurse = await _unitOfWork.NurseProfileRepository
                        .AnyAsync(n => n.UserId == id);

                if (!isNurse)
                    return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Nhân viên không phải là y tá."));
                var records = await _unitOfWork.CheckupRecordRepository
                            .GetQueryable()
                            .Include(r => r.CounselingAppointments)
                            .Where(r => !r.IsDeleted && r.ExaminedByNurseId == id)
                            .ToListAsync();

                if (records == null || !records.Any())
                {
                    return ApiResult<List<CheckupRecordRespondDTO?>>.Success(new List<CheckupRecordRespondDTO?>(), "Không có hồ sơ kiểm tra nào được tìm thấy cho nhân viên này.");
                }
                var result = records.Select(CheckupRecordMappings.MapToRespondDTO).ToList();
                return ApiResult<List<CheckupRecordRespondDTO?>>.Success(result, "Lấy bản ghi thành công với staff id : " + id);
            }
            catch (Exception ex)
            {
                return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Lỗi khi lấy bản ghi kiểm tra: " + ex.Message));
            }
        }
        public async Task<ApiResult<List<CheckupRecordRespondDTO?>>> GetAllByStudentCodeAsync(string studentCode)
        {
            try
            {
                var student = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.StudentCode == studentCode && !s.IsDeleted);

                if (student == null)
                    return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Không tìm thấy học sinh!"));

                var records = await _unitOfWork.CheckupRecordRepository
                    .GetQueryable()
                    .Include(r => r.CounselingAppointments)
                    .Where(r => r.Schedule.StudentId == student.Id && !r.IsDeleted)
                    .ToListAsync();

                var result = records.Select(CheckupRecordMappings.MapToRespondDTO).ToList();
                return ApiResult<List<CheckupRecordRespondDTO?>>.Success(result, "lấy hồ sơ theo mã học sinh thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Lỗi khi lấy hồ sơ theo mã học sinh: " + ex.Message));
            }
        }

        public async Task<ApiResult<CheckupRecordRespondDTO?>> GetByIdAsync(Guid id)
        {
            try
            {
                var record = await _unitOfWork.CheckupRecordRepository
                    .GetQueryable()
                    .Include(r => r.CounselingAppointments)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (record == null)
                    return ApiResult<CheckupRecordRespondDTO?>.Failure(new Exception("Không tìm thấy hồ sơ kiểm tra!"));

                var response = CheckupRecordMappings.MapToRespondDTO(record);
                return ApiResult<CheckupRecordRespondDTO?>.Success(response,"Lấy thông tin thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<CheckupRecordRespondDTO?>.Failure(new Exception("Lỗi khi lấy hồ sơ kiểm tra theo ID: " + ex.Message));
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteAsync(Guid id)
        {
            try
            {
                var record = await _unitOfWork.CheckupRecordRepository
                    .GetQueryable()
                    .Include(r => r.CounselingAppointments)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (record == null || record.IsDeleted)
                    return ApiResult<bool>.Failure(new Exception($"Không tìm thấy hồ sơ kiểm tra hoặc đã bị xoá: {id}"));

                record.IsDeleted = true;
                record.DeletedAt = _currentTime.GetVietnamTime();
                record.DeletedBy = _currentUserService.GetUserId() ?? Guid.Empty;

                foreach (var ca in record.CounselingAppointments)
                {
                    ca.IsDeleted = true;
                    ca.DeletedAt = _currentTime.GetVietnamTime();
                    ca.DeletedBy = _currentUserService.GetUserId() ?? Guid.Empty;
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, $"Xoá mềm hồ sơ {id} và các lịch hẹn tư vấn liên quan thành công!");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(new Exception("Lỗi khi xoá mềm hồ sơ: " + ex.Message));
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteRangeAsync(List<Guid> ids)
        {
            try
            {
                var records = await _unitOfWork.CheckupRecordRepository
                    .GetQueryable()
                    .Include(r => r.CounselingAppointments)
                    .Where(r => ids.Contains(r.Id) && !r.IsDeleted)
                    .ToListAsync();

                var deletedIds = new List<Guid>();
                var failedIds = ids.Except(records.Select(r => r.Id)).ToList();

                foreach (var record in records)
                {
                    record.IsDeleted = true;
                    record.DeletedAt = _currentTime.GetVietnamTime();
                    record.DeletedBy = _currentUserService.GetUserId() ?? Guid.Empty;
                    deletedIds.Add(record.Id);

                    foreach (var ca in record.CounselingAppointments)
                    {
                        ca.IsDeleted = true;
                        ca.DeletedAt = _currentTime.GetVietnamTime();
                        ca.DeletedBy = _currentUserService.GetUserId() ?? Guid.Empty;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var message = $"Xoá mềm thành công: [{string.Join(", ", deletedIds)}]. ";
                if (failedIds.Any())
                {
                    message += $"Không xoá được: [{string.Join(", ", failedIds)}].";
                }

                return ApiResult<bool>.Success(true, message);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(new Exception("Lỗi khi xoá mềm hàng loạt hồ sơ: " + ex.Message));
            }
        }
    }
}
