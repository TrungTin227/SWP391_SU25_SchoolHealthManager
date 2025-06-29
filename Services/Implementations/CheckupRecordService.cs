using Azure;
using BusinessObjects;
using DTOs.CheckUpRecordDTOs.Requests;
using DTOs.CheckUpRecordDTOs.Responds;
using DTOs.CounselingAppointmentDTOs.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            var records = await _unitOfWork.CheckupRecordRepository
                    .GetQueryable()
                    .Where(r => !r.IsDeleted && r.ExaminedByNurseId == id)
                    .ToListAsync();

            if (records == null || !records.Any())
            {
                return ApiResult<List<CheckupRecordRespondDTO?>>.Success(new List<CheckupRecordRespondDTO?>(), "Không có hồ sơ kiểm tra nào được tìm thấy cho nhân viên này.");
            }
            var result = _mapper.Map<List<CheckupRecordRespondDTO>>(records);
            return ApiResult<List<CheckupRecordRespondDTO?>>.Success(result);
        }

        public async Task<ApiResult<List<CheckupRecordRespondDTO?>>> GetAllByStudentCodeAsync(string studentId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<CheckupRecordRespondDTO?>> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<bool>> SoftDeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<bool>> SoftDeleteRangeAsync(List<Guid> id)
        {
            throw new NotImplementedException();
        }
    }
}
