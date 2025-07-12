using Azure;
using BusinessObjects;
using DTOs.CheckUpRecordDTOs.Requests;
using DTOs.CheckUpRecordDTOs.Responds;
using DTOs.CounselingAppointmentDTOs.Requests;
using DTOs.CounselingAppointmentDTOs.Responds;
using DTOs.GlobalDTO.Respond;
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
        #region Create Checkup Record
        public async Task<ApiResult<CheckupRecordRespondDTO>> CreateCheckupRecordAsync(CreateCheckupRecordRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra lịch khám
                var schedule = await _unitOfWork.CheckupScheduleRepository.GetByIdAsync(request.ScheduleId)
                             ?? throw new Exception("Lịch khám không tồn tại!!");
                if (schedule.IsDeleted)
                    throw new Exception("Lịch khám đã bị xóa!!");

                if (!IsWithinWorkingHours(request.ExaminedAt))
                    throw new Exception("Lịch khám không nằm trong giờ làm việc của trường!!");

                if (schedule.ParentConsentStatus != CheckupScheduleStatus.Approved)
                    throw new Exception("Phụ huynh chưa đồng ý lịch khám này!!");

                var recordExists = await _unitOfWork.CheckupRecordRepository
                    .AnyAsync(x => x.ScheduleId == request.ScheduleId);
                if (recordExists)
                    throw new Exception("Lịch khám này đã có hồ sơ học sinh rồi!");

                // 2. Kiểm tra học sinh
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(schedule.StudentId)
                            ?? throw new Exception("Học sinh không tồn tại!!");
                if (student.IsDeleted)
                    throw new Exception("Học sinh đã bị xóa!");

                // 3. Kiểm tra y tá

                var nurse = await _unitOfWork.NurseProfileRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(n => n.UserId == request.ExaminedByNurseId && !n.IsDeleted);

                if (request.ExaminedByNurseId.HasValue && nurse == null)
                    throw new Exception("Y tá không tồn tại hoặc đã bị xóa!");

                if (request.ExaminedByNurseId.HasValue)
                {
                    var isNurse = await _unitOfWork.NurseProfileRepository
                        .AnyAsync(n => n.UserId == request.ExaminedByNurseId && !n.IsDeleted);
                    if (!isNurse)
                        throw new Exception("Y tá không có quyền cập nhật hồ sơ này!");
                }

                // 4. Tạo hồ sơ khám
                var checkupRecord = CheckupRecordMappings.MapToEntity(request, student);
                checkupRecord.Id = Guid.NewGuid();
                await _unitOfWork.CheckupRecordRepository.AddAsync(checkupRecord);

                // Xoá mềm lịch khám đã xử lý
                schedule.Record = checkupRecord;
                schedule.IsDeleted = true;
                await _unitOfWork.CheckupScheduleRepository.UpdateAsync(schedule);

                // 4. Tạo lịch tư vấn nếu cần
                var counselingRequired = request.Status == CheckupRecordStatus.RequiresFollowUp;
                var hasAppointments = request.CounselingAppointment?.Any() == true;

                if (counselingRequired && hasAppointments)
                {
                    foreach (var caDto in request.CounselingAppointment)
                    {
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

                        _logger.LogInformation("📅 Tạo lịch hẹn tư vấn với purpose = {Purpose}", appointmentDto.Purpose);

                        var result = await _counselingAppointmentService.CreateCounselingAppointmentAsync(appointmentDto);
                        if (!result.IsSuccess)
                            throw new Exception("Tạo appointment thất bại: " + result.Message);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                var response = CheckupRecordMappings.MapToRespondDTO(checkupRecord);

                if (!counselingRequired && hasAppointments)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Success(
                        response,
                        "Tạo hồ sơ thành công, nhưng không tạo lịch hẹn tư vấn vì status không phải RequiresFollowUp!!"
                    );
                }

                return ApiResult<CheckupRecordRespondDTO>.Success(response, "Tạo hồ sơ kiểm tra thành công!!");
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Tạo hồ sơ kiểm tra thất bại!! " + e.Message));
            }
        }


        public async Task<ApiResult<CheckupRecordRespondDTO>> UpdateCheckupRecordAsync(UpdateCheckupRecordRequestDTO request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var result = await UpdateCheckupRecordCoreAsync(request);
                if (!result.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return result;
                }

                await _unitOfWork.CommitTransactionAsync();
                return ApiResult<CheckupRecordRespondDTO>.Success(result.Data, "Cập nhật hồ sơ kiểm tra thành công!");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Cập nhật hồ sơ thất bại!! " + ex.Message));
            }
        }

        public async Task<ApiResult<IReadOnlyList<CheckupRecordRespondDTO>>> UpdateCheckupRecordsAsync(IEnumerable<UpdateCheckupRecordRequestDTO> requests)
        {
            await _unitOfWork.BeginTransactionAsync();

            var updatedList = new List<CheckupRecordRespondDTO>();

            try
            {
                foreach (var req in requests)
                {
                    var singleResult = await UpdateCheckupRecordCoreAsync(req);

                    if (!singleResult.IsSuccess)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResult<IReadOnlyList<CheckupRecordRespondDTO>>
                            .Failure(new Exception($"Update failed for record {req.Id}: {singleResult.Message}"));
                    }

                    updatedList.Add(singleResult.Data);
                }

                await _unitOfWork.CommitTransactionAsync();
                return ApiResult<IReadOnlyList<CheckupRecordRespondDTO>>
                    .Success(updatedList, $"Đã cập nhật {updatedList.Count} hồ sơ thành công!");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResult<IReadOnlyList<CheckupRecordRespondDTO>>
                    .Failure(new Exception("Bulk update thất bại: " + ex.Message));
            }
        }

        public async Task<ApiResult<CheckupRecordRespondDTO>> UpdateCheckupRecordStatusAsync(Guid id, CheckupRecordStatus newStatus)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                if (currentUserId == null)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Không xác định được người dùng hiện tại!"));
                }

                var record = await _unitOfWork.CheckupRecordRepository.GetByIdAsync(id);
                if (record == null || record.IsDeleted)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Hồ sơ kiểm tra không tồn tại hoặc đã bị xóa!!"));
                }

                if (record.Status == CheckupRecordStatus.RequiresFollowUp && newStatus != CheckupRecordStatus.RequiresFollowUp)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Không thể chuyển trạng thái hồ sơ cần tái khám sang trạng thái khác!"));
                }

                // Không cho update lùi status
                if ((int)newStatus < (int)record.Status)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Không được cập nhật lùi trạng thái hồ sơ!"));
                }

                record.Status = newStatus;
                record.UpdatedAt = DateTime.UtcNow;
                record.UpdatedBy = currentUserId.Value;

                await _unitOfWork.CheckupRecordRepository.UpdateAsync(record);
                await _unitOfWork.SaveChangesAsync();

                var response = CheckupRecordMappings.MapToRespondDTO(record);
                return ApiResult<CheckupRecordRespondDTO>.Success(response, "Cập nhật trạng thái hồ sơ thành công!");
            }
            catch (Exception ex)
            {
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Cập nhật trạng thái thất bại!! " + ex.Message));
            }
        }

        #endregion
        #region Get Checkup Record
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

                if (records == null || !records.Any())
                    return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Không có hồ sơ sức khỏe nào với mã sinh viên :"+studentCode));

                var result = records.Select(CheckupRecordMappings.MapToRespondDTO).ToList();
                return ApiResult<List<CheckupRecordRespondDTO?>>.Success(result, "lấy hồ sơ theo mã học sinh thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Lỗi khi lấy hồ sơ theo mã học sinh: " + ex.Message));
            }
        }

        public async Task<ApiResult<List<CheckupRecordRespondDTO?>>> GetAllByStudentIdAsync(Guid studentId)
        {
            try
            {
                var student = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

                if (student == null)
                    return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Không tìm thấy học sinh!"));

                var records = await _unitOfWork.CheckupRecordRepository
                    .GetQueryable()
                    .Include(r => r.CounselingAppointments)
                    .Where(r => r.Schedule.StudentId == student.Id && !r.IsDeleted)
                    .ToListAsync();

                var result = records.Select(CheckupRecordMappings.MapToRespondDTO).ToList();

                if (result == null || !result.Any())
                    return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Không có hồ sơ kiểm tra nào được tìm thấy cho học sinh này."));

                return ApiResult<List<CheckupRecordRespondDTO?>>.Success(result, "Lấy hồ sơ theo ID học sinh thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<CheckupRecordRespondDTO?>>.Failure(new Exception("Lỗi khi lấy hồ sơ theo ID học sinh: " + ex.Message));
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
        #endregion
        #region Delete and Restore Checkup Record
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

        public async Task<ApiResult<int>> RestoreRangeAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return ApiResult<int>.Failure(new Exception("Danh sách ID không được để trống."));

            try
            {
                var records = await _unitOfWork.CheckupRecordRepository
                    .GetQueryable()
                    .IgnoreQueryFilters()
                    .Include(r => r.CounselingAppointments)
                    .Where(r => ids.Contains(r.Id) && r.IsDeleted)
                    .ToListAsync();

                if (!records.Any())
                    return ApiResult<int>.Failure(new Exception("Không tìm thấy hồ sơ nào phù hợp để khôi phục."));

                var currentTime = _currentTime.GetVietnamTime();
                var currentUser = _currentUserService.GetUserId() ?? Guid.Empty;

                foreach (var record in records)
                {
                    record.IsDeleted = false;
                    record.DeletedAt = null;
                    record.DeletedBy = null;
                    record.UpdatedAt = currentTime;
                    record.UpdatedBy = currentUser;

                    foreach (var ca in record.CounselingAppointments.Where(ca => ca.IsDeleted))
                    {
                        ca.IsDeleted = false;
                        ca.DeletedAt = null;
                        ca.DeletedBy = null;
                        ca.UpdatedAt = currentTime;
                        ca.UpdatedBy = currentUser;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<int>.Success(records.Count, $"Khôi phục thành công {records.Count} hồ sơ khám!");
            }
            catch (Exception ex)
            {
                return ApiResult<int>.Failure(new Exception("Lỗi khi khôi phục danh sách hồ sơ khám: " + ex.Message));
            }
        }


        #endregion
        #region Private Methods 
        private async Task<ApiResult<CheckupRecordRespondDTO>> UpdateCheckupRecordCoreAsync(UpdateCheckupRecordRequestDTO request)
        {
            var record = await _unitOfWork.CheckupRecordRepository.GetByIdAsync(request.Id);
            if (record == null || record.IsDeleted)
            {
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Hồ sơ không tồn tại hoặc đã bị xóa!"));
            }

            if (record.Status == CheckupRecordStatus.Completed)
            {
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Không thể cập nhật hồ sơ đã hoàn thành!"));
            }

            if (record.Status == CheckupRecordStatus.RequiresFollowUp && request.Status != CheckupRecordStatus.RequiresFollowUp)
            {
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Không thể cập nhật hồ sơ cần tái khám sang trạng thái khác!"));
            }

            var currentUserId = _currentUserService.GetUserId();
            if (currentUserId == null)
            {
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Không xác định được người dùng hiện tại!"));
            }

            bool isUpdated = false;

            if (request.HeightCm.HasValue)
            {
                record.HeightCm = request.HeightCm.Value;
                isUpdated = true;
            }

            if (request.WeightKg.HasValue)
            {
                record.WeightKg = request.WeightKg.Value;
                isUpdated = true;
            }

            if (request.VisionLeft.HasValue)
            {
                record.VisionLeft = request.VisionLeft.Value;
                isUpdated = true;
            }

            if (request.VisionRight.HasValue)
            {
                record.VisionRight = request.VisionRight.Value;
                isUpdated = true;
            }

            if (request.Hearing.HasValue)
            {
                record.Hearing = request.Hearing.Value;
                isUpdated = true;
            }

            if (request.BloodPressureDiastolic.HasValue)
            {
                record.BloodPressureDiastolic = request.BloodPressureDiastolic.Value;
                isUpdated = true;
            }

            if (request.ExaminedByNurseId.HasValue)
            {
                var isNurse = await _unitOfWork.NurseProfileRepository
                    .AnyAsync(n => n.UserId == request.ExaminedByNurseId && !n.IsDeleted);

                if (!isNurse)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Y tá không có quyền cập nhật hồ sơ này!"));
                }

                record.ExaminedByNurseId = request.ExaminedByNurseId.Value;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Remarks))
            {
                record.Remarks = request.Remarks;
                isUpdated = true;
            }

            if (request.Status.HasValue && request.Status.Value != record.Status)
            {
                record.Status = request.Status.Value;
                isUpdated = true;
            }

            if (!isUpdated)
            {
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Không có trường nào được cập nhật!"));
            }

            record.UpdatedAt = DateTime.UtcNow;
            record.UpdatedBy = currentUserId.Value;

            await _unitOfWork.CheckupRecordRepository.UpdateAsync(record);

            var response = CheckupRecordMappings.MapToRespondDTO(record);
            return ApiResult<CheckupRecordRespondDTO>.Success(response,"Update Checkup Record thành công!!");
        }

        #endregion
    }
}
