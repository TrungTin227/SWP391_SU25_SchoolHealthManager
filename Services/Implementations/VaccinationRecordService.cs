using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs.VaccinationRecordDTOs.Request;
using DTOs.VaccinationRecordDTOs.Response;
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccinationRecordService
        : BaseService<VaccinationRecord, Guid>, IVaccinationRecordService
    {
        private readonly IVaccinationRecordRepository _recordRepository;
        private readonly ILogger<VaccinationRecordService> _logger;

        public VaccinationRecordService(
            IVaccinationRecordRepository recordRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            ILogger<VaccinationRecordService> logger)
            : base(recordRepository, currentUserService, unitOfWork, currentTime)
        {
            _recordRepository = recordRepository;
            _logger = logger;
        }

        #region CRUD

        public async Task<ApiResult<bool>> CreateAsync(CreateVaccinationRecordRequest request)
        {
            try
            {
                var exists = await _recordRepository.HasDuplicateEntryAsync(request.StudentId, request.ScheduleId);
                if (exists)
                {
                    return ApiResult<bool>.Failure(new InvalidOperationException("Học sinh đã có phiếu tiêm trong lịch này."));
                }

                var record = new VaccinationRecord
                {
                    Id = Guid.NewGuid(),
                    StudentId = request.StudentId,
                    ScheduleId = request.ScheduleId,
                    SessionStudentId = request.SessionStudentId,
                    VaccineLotId = request.VaccineLotId,
                    AdministeredDate = request.AdministeredDate,
                    VaccinatedById = request.VaccinatedById,
                    VaccinatedAt = request.VaccinatedAt,
                    ReactionFollowup24h = request.ReactionFollowup24h,
                    ReactionFollowup72h = request.ReactionFollowup72h,
                    ReactionSeverity = (VaccinationReactionSeverity)request.ReactionSeverity,
                    VaccineTypeId = request.VaccineTypeId,
                    CreatedAt = _currentTime.GetCurrentTime(),
                    UpdatedAt = _currentTime.GetCurrentTime(),
                    IsDeleted = false
                };

                var created = await base.CreateAsync(record);
                return ApiResult<bool>.Success(created != null, "Tạo phiếu tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiếu tiêm");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateVaccinationRecordRequest request)
        {
            try
            {
                var record = await _recordRepository.GetByIdAsync(id);
                if (record == null || record.IsDeleted)
                {
                    return ApiResult<bool>.Failure(new KeyNotFoundException("Không tìm thấy phiếu tiêm."));
                }

                record.UpdatedAt = _currentTime.GetCurrentTime();

                if (request.AdministeredDate.HasValue)
                    record.AdministeredDate = request.AdministeredDate.Value;

                if (request.VaccinatedAt.HasValue)
                    record.VaccinatedAt = request.VaccinatedAt.Value;

                if (request.ReactionFollowup24h.HasValue)
                    record.ReactionFollowup24h = request.ReactionFollowup24h.Value;

                if (request.ReactionFollowup72h.HasValue)
                    record.ReactionFollowup72h = request.ReactionFollowup72h.Value;

                if (request.ReactionSeverity.HasValue)
                    record.ReactionSeverity = (VaccinationReactionSeverity)request.ReactionSeverity.Value;

                var updated = await base.UpdateAsync(record);
                if (updated != null)
                {
                    _logger.LogInformation("Cập nhật phiếu tiêm thành công: {RecordId}", id);
                    return ApiResult<bool>.Success(true, "Cập nhật phiếu tiêm thành công");
                }

                return ApiResult<bool>.Failure(new InvalidOperationException("Không thể cập nhật phiếu tiêm."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu tiêm {RecordId}", id);
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> DeleteAsync(Guid id, Guid deletedBy)
        {
            try
            {
                var record = await _recordRepository.GetByIdAsync(id);
                if (record == null || record.IsDeleted)
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy phiếu tiêm."));

                record.IsDeleted = true;
                record.DeletedAt = _currentTime.GetCurrentTime();
                record.DeletedBy = deletedBy;

                var deleted = await base.UpdateAsync(record);
                if (deleted != null)
                {
                    _logger.LogInformation("Xóa mềm phiếu tiêm thành công: {RecordId}", id);
                    return ApiResult<bool>.Success(true, "Xóa phiếu tiêm thành công");
                }

                return ApiResult<bool>.Failure(new InvalidOperationException("Không thể xóa phiếu tiêm."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm phiếu tiêm");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccinationRecord?>> GetByIdAsync(Guid id)
        {
            try
            {
                var record = await _recordRepository.GetRecordWithDetailsAsync(id);
                return ApiResult<VaccinationRecord?>.Success(record, "Lấy phiếu tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy phiếu tiêm theo ID");
                return ApiResult<VaccinationRecord?>.Failure(ex);
            }
        }

        #endregion

        #region Queries

        public async Task<ApiResult<PagedList<VaccinationRecord>>> GetRecordsByScheduleAsync(Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var records = await _recordRepository.GetRecordsByScheduleAsync(scheduleId, pageNumber, pageSize, searchTerm);
                return ApiResult<PagedList<VaccinationRecord>>.Success(records, "Lấy danh sách phiếu tiêm theo lịch thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiêm theo lịch");
                return ApiResult<PagedList<VaccinationRecord>>.Failure(ex);
            }
        }

        public async Task<ApiResult<PagedList<VaccinationRecord>>> GetRecordsByStudentAsync(Guid studentId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var records = await _recordRepository.GetRecordsByStudentAsync(studentId, pageNumber, pageSize, searchTerm);
                return ApiResult<PagedList<VaccinationRecord>>.Success(records, "Lấy danh sách phiếu tiêm theo học sinh thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiêm theo học sinh");
                return ApiResult<PagedList<VaccinationRecord>>.Failure(ex);
            }
        }

        #endregion
    }
}
