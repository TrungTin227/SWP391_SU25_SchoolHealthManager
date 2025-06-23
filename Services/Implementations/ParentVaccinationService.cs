using DTOs.ParentVaccinationDTOs.Request;
using DTOs.ParentVaccinationDTOs.Response;
using DTOs.VaccinationScheduleDTOs.Response;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class ParentVaccinationService : BaseService<SessionStudent, Guid>, IParentVaccinationService
    {
        private readonly IParentVaccinationRepository _parentVaccinationRepository;
        private readonly ILogger<ParentVaccinationService> _logger;

        public ParentVaccinationService(
            IGenericRepository<SessionStudent, Guid> repository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            IParentVaccinationRepository parentVaccinationRepository,
            ILogger<ParentVaccinationService> logger)
            : base(repository, currentUserService, unitOfWork, currentTime)
        {
            _parentVaccinationRepository = parentVaccinationRepository;
            _logger = logger;
        }

        public async Task<ApiResult<PagedList<ParentVaccinationScheduleResponseDTO>>> GetVaccinationSchedulesByStatusAsync(
            ParentActionStatus status, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserIdOrThrow();

                var schedules = await _parentVaccinationRepository
                    .GetParentVaccinationSchedulesAsync(currentUserId, status, pageNumber, pageSize);

                var responseDTOs = new List<ParentVaccinationScheduleResponseDTO>();

                foreach (var schedule in schedules)
                {
                    var responseDTO = ParentVaccinationMapper.MapToScheduleResponseDTO(schedule, currentUserId);
                    responseDTOs.Add(responseDTO);
                }

                var result = new PagedList<ParentVaccinationScheduleResponseDTO>(
                    responseDTOs,
                    schedules.MetaData.TotalCount,
                    schedules.MetaData.CurrentPage,
                    schedules.MetaData.PageSize);

                return ApiResult<PagedList<ParentVaccinationScheduleResponseDTO>>.Success(
                    result,
                    $"Lấy danh sách lịch tiêm thành công. Tổng số: {result.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tiêm cho phụ huynh");
                return ApiResult<PagedList<ParentVaccinationScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<ParentVaccinationScheduleResponseDTO>> GetVaccinationScheduleDetailAsync(Guid scheduleId)
        {
            try
            {
                var currentUserId = GetCurrentUserIdOrThrow();

                var sessionStudents = await _parentVaccinationRepository
                    .GetParentSessionStudentsAsync(currentUserId, scheduleId);

                if (!sessionStudents.Any())
                {
                    return ApiResult<ParentVaccinationScheduleResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy lịch tiêm hoặc bạn không có quyền truy cập"));
                }

                var schedule = sessionStudents.First().VaccinationSchedule;
                var responseDTO = ParentVaccinationMapper.MapToScheduleDetailResponseDTO(schedule, sessionStudents);

                return ApiResult<ParentVaccinationScheduleResponseDTO>.Success(
                    responseDTO, "Lấy chi tiết lịch tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết lịch tiêm {ScheduleId}", scheduleId);
                return ApiResult<ParentVaccinationScheduleResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> SubmitConsentAsync(ParentConsentRequestDTO request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var currentUserId = GetCurrentUserIdOrThrow();

                    // Kiểm tra quyền truy cập
                    var canAccess = await _parentVaccinationRepository
                        .CanParentAccessSessionAsync(currentUserId, request.SessionStudentId);

                    if (!canAccess)
                    {
                        return ApiResult<bool>.Failure(
                            new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này"));
                    }

                    var sessionStudent = await _unitOfWork.SessionStudentRepository
                        .GetByIdAsync(request.SessionStudentId);

                    if (sessionStudent == null)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException("Không tìm thấy thông tin đăng ký tiêm chủng"));
                    }

                    // Kiểm tra trạng thái hiện tại
                    if (sessionStudent.ConsentStatus == ParentConsentStatus.Approved ||
                        sessionStudent.ConsentStatus == ParentConsentStatus.Rejected)
                    {
                        return ApiResult<bool>.Failure(
                            new InvalidOperationException("Bạn đã thực hiện đồng ý/từ chối cho lịch tiêm này"));
                    }

                    // Kiểm tra hạn chót
                    if (sessionStudent.ConsentDeadline.HasValue &&
                        _currentTime.GetVietnamTime() > sessionStudent.ConsentDeadline.Value)
                    {
                        return ApiResult<bool>.Failure(
                            new InvalidOperationException("Đã hết hạn để thực hiện đồng ý/từ chối"));
                    }

                    // Cập nhật thông tin đồng ý
                    sessionStudent.ConsentStatus = request.ConsentStatus;
                    sessionStudent.ParentSignedAt = _currentTime.GetVietnamTime();
                    sessionStudent.ParentNotes = request.ParentNotes;
                    sessionStudent.ParentSignature = request.ParentSignature;
                    sessionStudent.UpdatedAt = _currentTime.GetVietnamTime();
                    sessionStudent.UpdatedBy = currentUserId;

                    await _unitOfWork.SessionStudentRepository.UpdateAsync(sessionStudent);
                    await _unitOfWork.SaveChangesAsync();

                    var statusText = request.ConsentStatus == ParentConsentStatus.Approved ? "đồng ý" : "từ chối";
                    _logger.LogInformation("Phụ huynh {UserId} đã {Status} cho SessionStudent {SessionId}",
                        currentUserId, statusText, request.SessionStudentId);

                    return ApiResult<bool>.Success(true, $"Đã ghi nhận {statusText} tiêm chủng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi ghi nhận đồng ý tiêm chủng");
                    return ApiResult<bool>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<bool>> SubmitBatchConsentAsync(BatchParentConsentRequestDTO request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var currentUserId = GetCurrentUserIdOrThrow();
                    var successCount = 0;
                    var errors = new List<string>();

                    foreach (var consent in request.Consents)
                    {
                        try
                        {
                            var result = await SubmitConsentAsync(consent);
                            if (result.IsSuccess)
                            {
                                successCount++;
                            }
                            else
                            {
                                errors.Add($"SessionStudent {consent.SessionStudentId}: {result.Message}");
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"SessionStudent {consent.SessionStudentId}: {ex.Message}");
                        }
                    }

                    var message = $"Đã xử lý {successCount}/{request.Consents.Count} yêu cầu đồng ý thành công";
                    if (errors.Any())
                    {
                        message += $". Lỗi: {string.Join("; ", errors)}";
                    }

                    return ApiResult<bool>.Success(successCount > 0, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý hàng loạt đồng ý tiêm chủng");
                    return ApiResult<bool>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<List<ParentVaccinationHistoryResponseDTO>>> GetVaccinationHistoryAsync()
        {
            try
            {
                var currentUserId = GetCurrentUserIdOrThrow();

                var records = await _parentVaccinationRepository
                    .GetParentVaccinationHistoryAsync(currentUserId);

                var groupedByStudent = records
                    .GroupBy(vr => new { vr.StudentId, vr.Student.FullName, vr.Student.StudentCode })
                    .Select(g => new ParentVaccinationHistoryResponseDTO
                    {
                        StudentId = g.Key.StudentId,
                        StudentName = g.Key.FullName,
                        StudentCode = g.Key.StudentCode,
                        VaccinationHistory = g.Select(ParentVaccinationMapper.MapToHistoryRecordDTO).ToList()
                    })
                    .ToList();

                return ApiResult<List<ParentVaccinationHistoryResponseDTO>>.Success(
                    groupedByStudent, "Lấy lịch sử tiêm chủng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử tiêm chủng");
                return ApiResult<List<ParentVaccinationHistoryResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<ParentVaccinationHistoryResponseDTO>> GetStudentVaccinationHistoryAsync(Guid studentId)
        {
            try
            {
                var currentUserId = GetCurrentUserIdOrThrow();

                // Kiểm tra quyền truy cập
                var canAccess = await _parentVaccinationRepository
                    .CanParentAccessStudentAsync(currentUserId, studentId);

                if (!canAccess)
                {
                    return ApiResult<ParentVaccinationHistoryResponseDTO>.Failure(
                        new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin học sinh này"));
                }

                var records = await _parentVaccinationRepository
                    .GetStudentVaccinationHistoryAsync(currentUserId, studentId);

                if (!records.Any())
                {
                    var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);
                    return ApiResult<ParentVaccinationHistoryResponseDTO>.Success(
                        new ParentVaccinationHistoryResponseDTO
                        {
                            StudentId = studentId,
                            StudentName = student?.FullName ?? "",
                            StudentCode = student?.StudentCode ?? "",
                            VaccinationHistory = new List<VaccinationHistoryRecordDTO>()
                        }, "Học sinh chưa có lịch sử tiêm chủng");
                }

                var firstRecord = records.First();
                var response = new ParentVaccinationHistoryResponseDTO
                {
                    StudentId = studentId,
                    StudentName = firstRecord.Student.FullName,
                    StudentCode = firstRecord.Student.StudentCode,
                    VaccinationHistory = records.Select(ParentVaccinationMapper.MapToHistoryRecordDTO).ToList()
                };

                return ApiResult<ParentVaccinationHistoryResponseDTO>.Success(
                    response, "Lấy lịch sử tiêm chủng của học sinh thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử tiêm chủng của học sinh {StudentId}", studentId);
                return ApiResult<ParentVaccinationHistoryResponseDTO>.Failure(ex);
            }
        }

        //public async Task<ApiResult<bool>> ReportVaccinationReactionAsync(ReportVaccinationReactionRequestDTO request)
        //{
        //    return await _unitOfWork.ExecuteTransactionAsync(async () =>
        //    {
        //        try
        //        {
        //            var currentUserId = GetCurrentUserIdOrThrow();

        //            // Kiểm tra quyền truy cập
        //            var canAccess = await _parentVaccinationRepository
        //                .CanParentAccessStudentAsync(currentUserId, request.StudentId);

        //            if (!canAccess)
        //            {
        //                return ApiResult<bool>.Failure(
        //                    new UnauthorizedAccessException("Bạn không có quyền báo cáo cho học sinh này"));
        //            }

        //            var vaccinationRecord = await _unitOfWork.VaccinationRecordRepository
        //                .FirstOrDefaultAsync(vr => vr.Id == request.VaccinationRecordId &&
        //                                          vr.StudentId == request.StudentId);

        //            if (vaccinationRecord == null)
        //            {
        //                return ApiResult<bool>.Failure(
        //                    new KeyNotFoundException("Không tìm thấy bản ghi tiêm chủng"));
        //            }

        //            // Cập nhật thông tin phản ứng
        //            vaccinationRecord.ReactionSeverity = request.Severity;
        //            vaccinationRecord.ReactionNotes = request.Description;
        //            vaccinationRecord.ReactionReportedAt = _currentTime.GetVietnamTime();
        //            vaccinationRecord.UpdatedAt = _currentTime.GetVietnamTime();
        //            vaccinationRecord.UpdatedBy = currentUserId;

        //            await _unitOfWork.VaccinationRecordRepository.UpdateAsync(vaccinationRecord);
        //            await _unitOfWork.SaveChangesAsync();

        //            _logger.LogInformation("Phụ huynh {UserId} đã báo cáo phản ứng tiêm chủng cho học sinh {StudentId}, mức độ: {Severity}",
        //                currentUserId, request.StudentId, request.Severity);

        //            return ApiResult<bool>.Success(true, "Đã ghi nhận báo cáo phản ứng tiêm chủng thành công");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Lỗi khi báo cáo phản ứng tiêm chủng");
        //            return ApiResult<bool>.Failure(ex);
        //        }
        //    });
        //}

        public async Task<ApiResult<List<ParentVaccinationScheduleResponseDTO>>> GetPendingNotificationsAsync()
        {
            try
            {
                var pendingSchedules = await GetVaccinationSchedulesByStatusAsync(ParentActionStatus.PendingConsent, 1, 50);

                if (!pendingSchedules.IsSuccess)
                {
                    return ApiResult<List<ParentVaccinationScheduleResponseDTO>>.Failure(
                        new Exception(pendingSchedules.Message ?? "Unknown error"));
                }

                var notifications = pendingSchedules.Data.ToList();

                return ApiResult<List<ParentVaccinationScheduleResponseDTO>>.Success(
                    notifications, $"Có {notifications.Count} thông báo cần xử lý");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông báo chờ xử lý");
                return ApiResult<List<ParentVaccinationScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<ParentVaccinationSummaryDTO>> GetVaccinationSummaryAsync()
        {
            try
            {
                var currentUserId = GetCurrentUserIdOrThrow();

                var stats = await _parentVaccinationRepository
                    .GetParentVaccinationStatsAsync(currentUserId);

                // Lấy hoạt động gần đây
                var recentActivities = await GetVaccinationSchedulesByStatusAsync(ParentActionStatus.Completed, 1, 5);

                var summary = new ParentVaccinationSummaryDTO
                {
                    PendingConsentCount = stats.GetValueOrDefault(ParentActionStatus.PendingConsent, 0),
                    UpcomingVaccinationsCount = stats.GetValueOrDefault(ParentActionStatus.Approved, 0),
                    CompletedVaccinationsCount = stats.GetValueOrDefault(ParentActionStatus.Completed, 0),
                    RequiresFollowUpCount = stats.GetValueOrDefault(ParentActionStatus.RequiresFollowUp, 0),
                    RecentActivities = recentActivities.IsSuccess ? recentActivities.Data.ToList() : new List<ParentVaccinationScheduleResponseDTO>()
                };

                return ApiResult<ParentVaccinationSummaryDTO>.Success(
                    summary, "Lấy thống kê tổng quan thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê tổng quan");
                return ApiResult<ParentVaccinationSummaryDTO>.Failure(ex);
            }
        }
        // Add this inside ParentVaccinationService (preferably near the top, after fields)
        protected Guid GetCurrentUserIdOrThrow()
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");
            return userId.Value;
        }
    }
}