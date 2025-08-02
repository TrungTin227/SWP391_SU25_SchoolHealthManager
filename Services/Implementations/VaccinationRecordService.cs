using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs.VaccinationRecordDTOs.Request;
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

        public async Task<ApiResult<CreateVaccinationRecordResponse>> CreateAsync(CreateVaccinationRecordRequest request)
        {
            try
            {
                // 1. Kiểm tra duplicate
                var exists = await _recordRepository.HasDuplicateEntryAsync(request.StudentId, request.ScheduleId);
                if (exists)
                {
                    return ApiResult<CreateVaccinationRecordResponse>.Failure(
                        new InvalidOperationException("Học sinh đã có phiếu tiêm trong lịch này."));
                }

                // 2. Lấy thông tin lịch tiêm
                var schedule = await _unitOfWork.VaccinationScheduleRepository.GetByIdAsync(request.ScheduleId);
                if (schedule == null)
                {
                    return ApiResult<CreateVaccinationRecordResponse>.Failure(
                        new KeyNotFoundException("Không tìm thấy lịch tiêm."));
                }

                // 3. Kiểm tra thời hạn chiến dịch
                var now = _currentTime.GetVietnamTime();
                if (schedule.Campaign != null)
                {
                    if (now < schedule.Campaign.StartDate)
                    {
                        return ApiResult<CreateVaccinationRecordResponse>.Failure(
                            new InvalidOperationException("Chiến dịch tiêm chủng chưa bắt đầu."));
                    }

                    if (now > schedule.Campaign.EndDate)
                    {
                        return ApiResult<CreateVaccinationRecordResponse>.Failure(
                            new InvalidOperationException("Chiến dịch tiêm chủng đã kết thúc."));
                    }
                }

                // 4. Lấy thông tin session student
                var sessionStudent = await _unitOfWork.SessionStudentRepository
                    .GetByStudentAndScheduleAsync(request.StudentId, request.ScheduleId);
                if (sessionStudent == null)
                {
                    return ApiResult<CreateVaccinationRecordResponse>.Failure(
                        new KeyNotFoundException("Không tìm thấy mối liên kết học sinh và lịch tiêm."));
                }

                // 5. Kiểm tra consent status - CHỈ cho phép khi đã đồng ý
                if (sessionStudent.ConsentStatus != ParentConsentStatus.Approved)
                {
                    var consentStatusText = sessionStudent.ConsentStatus switch
                    {
                        ParentConsentStatus.Pending => "chưa phản hồi",
                        ParentConsentStatus.Sent => "đã được gửi thông báo và chưa phản hồi",
                        ParentConsentStatus.Rejected => "đã từ chối",
                        ParentConsentStatus.Expired => "đã hết hạn",
                        _ => "không xác định"
                    };

                    return ApiResult<CreateVaccinationRecordResponse>.Failure(
                        new InvalidOperationException($"Không thể tạo phiếu tiêm vì phụ huynh {consentStatusText}."));
                }

                // 6. Kiểm tra vaccine lot
                var reservedLot = await _recordRepository.TryReserveVaccineLotAsync(schedule.VaccinationTypeId);
                if (reservedLot == null)
                {
                    return ApiResult<CreateVaccinationRecordResponse>.Failure(
                        new InvalidOperationException("Không còn lô vaccine phù hợp còn tồn."));
                }

                // 7. Tạo vaccination record
                var record = new VaccinationRecord
                {
                    Id = Guid.NewGuid(),
                    SessionStudentId = sessionStudent.Id,
                    AdministeredDate = request.AdministeredDate,
                    VaccinatedById = request.VaccinatedById,
                    VaccinatedAt = request.VaccinatedAt,
                    ReactionFollowup24h = request.ReactionFollowup24h,
                    ReactionFollowup72h = request.ReactionFollowup72h,
                    ReactionSeverity = (VaccinationReactionSeverity)request.ReactionSeverity,
                    CreatedAt = _currentTime.GetCurrentTime(),
                    UpdatedAt = _currentTime.GetCurrentTime(),
                    IsDeleted = false
                };

                var created = await base.CreateAsync(record);

                if (created != null)
                {
                    // ✅ Set status = Completed
                    sessionStudent.Status = SessionStatus.Completed;
                    await _unitOfWork.SessionStudentRepository.UpdateAsync(sessionStudent);

                    // ✅ Save changes
                    await _unitOfWork.SaveChangesAsync();

                    var fullRecord = await _recordRepository.GetRecordWithDetailsAsync(created.Id);
                    if (fullRecord != null)
                    {
                        var responseDTO = VaccinationRecordMapper.MapToCreateResponseDTO(fullRecord);
                        return ApiResult<CreateVaccinationRecordResponse>.Success(responseDTO, "Tạo phiếu tiêm thành công");
                    }
                }

                return ApiResult<CreateVaccinationRecordResponse>.Failure(new Exception("Không thể tạo phiếu tiêm."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiếu tiêm");
                return ApiResult<CreateVaccinationRecordResponse>.Failure(ex);
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

        public async Task<ApiResult<CreateVaccinationRecordResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var record = await _recordRepository.GetRecordWithDetailsAsync(id);
                if (record == null)
                    return ApiResult<CreateVaccinationRecordResponse>.Failure(new KeyNotFoundException("Không tìm thấy phiếu tiêm."));

                var dto = VaccinationRecordMapper.MapToResponseDTO(record);
                return ApiResult<CreateVaccinationRecordResponse>.Success(dto, "Lấy phiếu tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy phiếu tiêm theo ID");
                return ApiResult<CreateVaccinationRecordResponse>.Failure(ex);
            }
        }

        #endregion

        #region Queries

        public async Task<ApiResult<PagedList<CreateVaccinationRecordResponse>>> GetRecordsByScheduleAsync(
      Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var records = await _recordRepository.GetRecordsByScheduleAsync(scheduleId, pageNumber, pageSize, searchTerm);

                var response = VaccinationRecordMapper.ToPagedResponseDTO(records);

                return ApiResult<PagedList<CreateVaccinationRecordResponse>>.Success(
                    response, "Lấy danh sách phiếu tiêm theo lịch thành công"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiêm theo lịch");
                return ApiResult<PagedList<CreateVaccinationRecordResponse>>.Failure(ex);
            }
        }

        public async Task<ApiResult<PagedList<CreateVaccinationRecordResponse>>> GetRecordsByStudentAsync(
            Guid studentId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var records = await _recordRepository.GetRecordsByStudentAsync(studentId, pageNumber, pageSize, searchTerm);

                var response = VaccinationRecordMapper.ToPagedResponseDTO(records);

                return ApiResult<PagedList<CreateVaccinationRecordResponse>>.Success(
                    response, "Lấy danh sách phiếu tiêm theo học sinh thành công"
                );                                                          
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiêm theo học sinh");
                return ApiResult<PagedList<CreateVaccinationRecordResponse>>.Failure(ex);
            }
        }

        public async Task<ApiResult<PagedList<CreateVaccinationRecordResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            try
            {
                var records = await _recordRepository.GetRecordsAsync(
                    null, null, null, null, null, pageNumber, pageSize, null
                );

                var response = VaccinationRecordMapper.ToPagedResponseDTO(records);
                return ApiResult<PagedList<CreateVaccinationRecordResponse>>.Success(response, "Lấy toàn bộ phiếu tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ phiếu tiêm");
                return ApiResult<PagedList<CreateVaccinationRecordResponse>>.Failure(ex);
            }
        }


        #endregion
    }
}