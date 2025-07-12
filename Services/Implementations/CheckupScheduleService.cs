using Microsoft.Extensions.Logging;
using Quartz;

namespace Services.Implementations
{
    public class CheckupScheduleService : BaseService<CheckupSchedule, Guid>, ICheckupScheduleService
    {
        private readonly ILogger<CheckupScheduleService> _logger;
        private readonly ISchoolHealthEmailService _emailService;

        public CheckupScheduleService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICurrentTime currentTime,
            ILogger<CheckupScheduleService> logger,
            ISchoolHealthEmailService emailService)
            : base(unitOfWork.CheckupScheduleRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<ApiResult<PagedList<CheckupScheduleResponseDTO>>> GetCheckupSchedulesAsync(
            int pageNumber, int pageSize, Guid? campaignId = null,
            CheckupScheduleStatus? status = null, string? searchTerm = null)
        {
            try
            {
                var schedulesPaged = await _unitOfWork.CheckupScheduleRepository
                    .GetCheckupSchedulesAsync(pageNumber, pageSize, campaignId, status, searchTerm);

                var responseDTOs = schedulesPaged.Select(MapToResponseDTO).ToList();

                var result = new PagedList<CheckupScheduleResponseDTO>(
                    responseDTOs,
                    schedulesPaged.MetaData.TotalCount,
                    schedulesPaged.MetaData.CurrentPage,
                    schedulesPaged.MetaData.PageSize);

                return ApiResult<PagedList<CheckupScheduleResponseDTO>>.Success(
                    result, "Lấy danh sách lịch khám thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch khám");
                return ApiResult<PagedList<CheckupScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<CheckupScheduleDetailResponseDTO>> GetCheckupScheduleByIdAsync(Guid id)
        {
            try
            {
                var schedule = await _unitOfWork.CheckupScheduleRepository.GetCheckupScheduleByIdAsync(id);
                if (schedule == null)
                {
                    return ApiResult<CheckupScheduleDetailResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy lịch khám với ID: {id}"));
                }

                var response = MapToDetailResponseDTO(schedule);
                return ApiResult<CheckupScheduleDetailResponseDTO>.Success(response,
                    "Lấy thông tin lịch khám thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin lịch khám với ID: {Id}", id);
                return ApiResult<CheckupScheduleDetailResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<CheckupScheduleResponseDTO>>> CreateCheckupSchedulesAsync(CreateCheckupScheduleRequest request)
        {
            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // Validate campaign exists
                var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(request.CampaignId);
                if (campaign == null)
                {
                    return ApiResult<List<CheckupScheduleResponseDTO>>.Failure(
                        new KeyNotFoundException("Không tìm thấy chiến dịch khám định kỳ"));
                }

                // Get student IDs based on request
                var studentIds = await GetStudentIdsFromRequest(request);
                if (!studentIds.Any())
                {
                    return ApiResult<List<CheckupScheduleResponseDTO>>.Failure(
                        new ArgumentException("Không tìm thấy học sinh nào để tạo lịch khám"));
                }

                // *** Load student data trước để có thể map đầy đủ ***
                var students = await _unitOfWork.StudentRepository.GetStudentsByIdsAsync(studentIds.ToList());
                var studentDict = students.ToDictionary(s => s.Id, s => s);

                // Create schedules
                var schedules = await CreateSchedulesFromStudentIds(request, studentIds);

                // Batch create
                await _unitOfWork.CheckupScheduleRepository.BatchCreateSchedulesAsync(schedules);
                await transaction.CommitAsync();

                foreach (var schedule in schedules)
                {
                    var student = await _unitOfWork.StudentRepository.GetByIdAsync(schedule.StudentId);
                    var parent = await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(student.ParentUserId);
                    if (student != null && !string.IsNullOrEmpty(parent.Email))
                    {
                        // Gọi hàm gửi mail
                        await _emailService.SendHealthCheckupNotificationAsync(parent.Email, student.FullName, schedule.ScheduledAt);
                        _logger.LogInformation("Đã gửi thông báo lịch khám cho phụ huynh {ParentEmail} của học sinh {StudentName}",
                            parent.Email, student.FullName);
                    }
                }

                    // Map với student data đã load
                    var responseDTOs = schedules.Select(schedule => MapToResponseDTOWithStudent(schedule, studentDict.GetValueOrDefault(schedule.StudentId), campaign)).ToList();

                return ApiResult<List<CheckupScheduleResponseDTO>>.Success(
                    responseDTOs, $"Tạo thành công {schedules.Count} lịch khám");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch khám cho campaign {CampaignId}", request.CampaignId);
                return ApiResult<List<CheckupScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<CheckupScheduleResponseDTO>> UpdateCheckupScheduleAsync(UpdateCheckupScheduleRequest request)
        {
            try
            {
                var schedule = await _unitOfWork.CheckupScheduleRepository.GetCheckupScheduleByIdAsync(request.Id);
                if (schedule == null)
                {
                    return ApiResult<CheckupScheduleResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy lịch khám"));
                }

                // Check for scheduling conflicts
                if (await _unitOfWork.CheckupScheduleRepository.HasConflictingScheduleAsync(
                    schedule.StudentId, request.ScheduledAt, request.Id))
                {
                    return ApiResult<CheckupScheduleResponseDTO>.Failure(
                        new InvalidOperationException("Học sinh đã có lịch khám trong khoảng thời gian này"));
                }

                // Update schedule
                schedule.ScheduledAt = request.ScheduledAt;
                schedule.ParentConsentStatus = request.ParentConsentStatus;
                schedule.SpecialNotes = request.SpecialNotes;
                schedule.UpdatedAt = _currentTime.GetVietnamTime();
                schedule.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Empty;

                if (request.ParentConsentStatus == CheckupScheduleStatus.Approved ||
                    request.ParentConsentStatus == CheckupScheduleStatus.Declined)
                {
                    schedule.ConsentReceivedAt = _currentTime.GetVietnamTime();
                }

                await _unitOfWork.CheckupScheduleRepository.UpdateAsync(schedule);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToResponseDTO(schedule);
                return ApiResult<CheckupScheduleResponseDTO>.Success(response, "Cập nhật lịch khám thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lịch khám {ScheduleId}", request.Id);
                return ApiResult<CheckupScheduleResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<CheckupScheduleResponseDTO>> UpdateConsentStatusAsync(UpdateConsentStatusRequest request)
        {
            try
            {
                var schedule = await _unitOfWork.CheckupScheduleRepository.GetCheckupScheduleByIdAsync(request.ScheduleId);
                if (schedule == null)
                {
                    return ApiResult<CheckupScheduleResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy lịch khám"));
                }

                schedule.ParentConsentStatus = request.ConsentStatus;
                schedule.ConsentReceivedAt = _currentTime.GetVietnamTime();
                schedule.SpecialNotes = request.Notes;
                schedule.UpdatedAt = _currentTime.GetVietnamTime();
                schedule.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Empty;

                await _unitOfWork.CheckupScheduleRepository.UpdateAsync(schedule);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToResponseDTO(schedule);
                return ApiResult<CheckupScheduleResponseDTO>.Success(response, "Cập nhật trạng thái đồng ý thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đồng ý {ScheduleId}", request.ScheduleId);
                return ApiResult<CheckupScheduleResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<BatchOperationResultDTO>> BatchUpdateScheduleStatusAsync(CheckupBatchUpdateStatusRequest request)
        {
            try
            {
                var result = new BatchOperationResultDTO
                {
                    TotalRequested = request.ScheduleIds.Count
                };

                var updatedCount = await _unitOfWork.CheckupScheduleRepository.BatchUpdateScheduleStatusAsync(
                    request.ScheduleIds, request.Status, _currentUserService.GetUserId() ?? Guid.Empty);

                await _unitOfWork.SaveChangesAsync();

                result.SuccessCount = updatedCount;
                result.FailureCount = request.ScheduleIds.Count - updatedCount;
                result.Message = $"Cập nhật trạng thái thành công cho {updatedCount}/{request.ScheduleIds.Count} lịch khám";

                // Add success IDs (simplified - in real scenario, you might want to track individual IDs)
                result.SuccessIds = request.ScheduleIds.Take(updatedCount).Select(id => id.ToString()).ToList();

                return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái hàng loạt");
                return ApiResult<BatchOperationResultDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<BatchOperationResultDTO>> BatchDeleteSchedulesAsync(CheckupBatchDeleteScheduleRequest request)
        {
            try
            {
                var result = new BatchOperationResultDTO
                {
                    TotalRequested = request.Ids.Count
                };

                var deletedCount = await _unitOfWork.CheckupScheduleRepository.BatchSoftDeleteAsync(
                    request.Ids, _currentUserService.GetUserId() ?? Guid.Empty);

                await _unitOfWork.SaveChangesAsync();

                result.SuccessCount = deletedCount;
                result.FailureCount = request.Ids.Count - deletedCount;
                result.Message = $"Xóa thành công {deletedCount}/{request.Ids.Count} lịch khám";
                result.SuccessIds = request.Ids.Take(deletedCount).Select(id => id.ToString()).ToList();

                return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hàng loạt lịch khám");
                return ApiResult<BatchOperationResultDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<BatchOperationResultDTO>> BatchRestoreSchedulesAsync(CheckupBatchRestoreScheduleRequest request)
        {
            try
            {
                var result = new BatchOperationResultDTO
                {
                    TotalRequested = request.Ids.Count
                };

                var restoredCount = await _unitOfWork.CheckupScheduleRepository.BatchRestoreAsync(
                    request.Ids, _currentUserService.GetUserId() ?? Guid.Empty);

                await _unitOfWork.SaveChangesAsync();

                result.SuccessCount = restoredCount;
                result.FailureCount = request.Ids.Count - restoredCount;
                result.Message = $"Khôi phục thành công {restoredCount}/{request.Ids.Count} lịch khám";
                result.SuccessIds = request.Ids.Take(restoredCount).Select(id => id.ToString()).ToList();

                return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khôi phục hàng loạt lịch khám");
                return ApiResult<BatchOperationResultDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<CheckupScheduleResponseDTO>>> GetSchedulesByCampaignAsync(Guid campaignId)
        {
            try
            {
                var schedules = await _unitOfWork.CheckupScheduleRepository.GetSchedulesByCampaignAsync(campaignId);
                var responseDTOs = schedules.Select(MapToResponseDTO).ToList();

                return ApiResult<List<CheckupScheduleResponseDTO>>.Success(responseDTOs,
                    "Lấy danh sách lịch khám theo chiến dịch thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch khám theo campaign {CampaignId}", campaignId);
                return ApiResult<List<CheckupScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<Dictionary<CheckupScheduleStatus, int>>> GetScheduleStatusStatisticsAsync(Guid? campaignId = null)
        {
            try
            {
                var statistics = await _unitOfWork.CheckupScheduleRepository.GetScheduleStatusStatisticsAsync(campaignId);
                return ApiResult<Dictionary<CheckupScheduleStatus, int>>.Success(statistics,
                    "Lấy thống kê trạng thái lịch khám thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê trạng thái lịch khám");
                return ApiResult<Dictionary<CheckupScheduleStatus, int>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<CheckupScheduleDetailResponseDTO>>> GetCheckupScheduleByStudentIdAsync(Guid id)
        {
            try
            {
                var result = await _unitOfWork.CheckupScheduleRepository.GetCheckupSchedulesByStudentIdAsync(id);
                if (result == null || !result.Any())
                {
                    return ApiResult<List<CheckupScheduleDetailResponseDTO>>.Failure(
                        new Exception("Không tìm thấy CheckupSchedule nào với Student Id: " + id));
                }

                // 👇 Map từng phần tử trong list
                var respond = result
                    .Select(MapToDetailResponseDTO)
                    .ToList();

                return ApiResult<List<CheckupScheduleDetailResponseDTO>>.Success(respond, "Lấy lịch khám theo Id học sinh thành công!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<CheckupScheduleDetailResponseDTO>>.Failure(new Exception("Lỗi khi lấy lịch khám theo Id học sinh!!"));
            }
        }

        #region Private Methods

        private async Task<HashSet<Guid>> GetStudentIdsFromRequest(CreateCheckupScheduleRequest request)
        {
            var studentIds = new HashSet<Guid>();

            // Add specific student IDs
            foreach (var studentId in request.StudentIds)
            {
                studentIds.Add(studentId);
            }

            // Add students by grade/section
            if (request.Grades.Any() || request.Sections.Any())
            {
                var students = await _unitOfWork.StudentRepository.GetStudentsByGradeAndSectionAsync(
                    request.Grades, request.Sections);

                foreach (var student in students)
                {
                    studentIds.Add(student.Id);
                }
            }

            return studentIds;
        }

        private async Task<List<CheckupSchedule>> CreateSchedulesFromStudentIds(
            CreateCheckupScheduleRequest request, HashSet<Guid> studentIds)
        {
            var schedules = new List<CheckupSchedule>();
            var currentVietnamTime = _currentTime.GetVietnamTime();
            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;

            // Bắt đầu từ 8:00 sáng ngày được chọn và tự động phân bổ 15 phút cho mỗi học sinh
            var baseScheduledTime = request.ScheduledDate.Date.AddHours(8);
            var currentTime = baseScheduledTime;
            const int intervalMinutes = 15;

            foreach (var studentId in studentIds)
            {
                // Check for conflicts
                if (await _unitOfWork.CheckupScheduleRepository.HasConflictingScheduleAsync(studentId, currentTime))
                {
                    _logger.LogWarning("Bỏ qua học sinh {StudentId} do xung đột lịch khám", studentId);
                    continue;
                }

                var schedule = new CheckupSchedule
                {
                    Id = Guid.NewGuid(),
                    CampaignId = request.CampaignId,
                    StudentId = studentId,
                    NotifiedAt = currentVietnamTime,
                    ScheduledAt = currentTime,
                    ParentConsentStatus = CheckupScheduleStatus.Pending,
                    SpecialNotes = request.Notes,
                    CreatedAt = currentVietnamTime,
                    UpdatedAt = currentVietnamTime,
                    CreatedBy = currentUserId,
                    UpdatedBy = currentUserId,
                    IsDeleted = false
                };

                schedules.Add(schedule);
                currentTime = currentTime.AddMinutes(intervalMinutes);
            }

            return schedules;
        }

        private CheckupScheduleResponseDTO MapToResponseDTO(CheckupSchedule schedule)
        {
            return new CheckupScheduleResponseDTO
            {
                Id = schedule.Id,
                CampaignId = schedule.CampaignId,
                CampaignName = schedule.Campaign?.Name ?? "",
                StudentId = schedule.StudentId,
                StudentName = schedule.Student?.FullName ?? "",
                StudentCode = schedule.Student?.StudentCode ?? "",
                Grade = schedule.Student?.Grade ?? "",
                Section = schedule.Student?.Section ?? "",
                NotifiedAt = schedule.NotifiedAt,
                ScheduledAt = schedule.ScheduledAt,
                ParentConsentStatus = schedule.ParentConsentStatus,
                ConsentReceivedAt = schedule.ConsentReceivedAt,
                SpecialNotes = schedule.SpecialNotes,
                HasRecord = schedule.Record != null,
                CreatedAt = schedule.CreatedAt
            };
        }

        private CheckupScheduleResponseDTO MapToResponseDTOWithStudent(CheckupSchedule schedule, Student? student, CheckupCampaign campaign)
        {
            return new CheckupScheduleResponseDTO
            {
                Id = schedule.Id,
                CampaignId = schedule.CampaignId,
                CampaignName = campaign?.Name ?? "",
                StudentId = schedule.StudentId,
                StudentName = student?.FullName ?? "",
                StudentCode = student?.StudentCode ?? "",
                Grade = student?.Grade ?? "",
                Section = student?.Section ?? "",
                NotifiedAt = schedule.NotifiedAt,
                ScheduledAt = schedule.ScheduledAt,
                ParentConsentStatus = schedule.ParentConsentStatus,
                ConsentReceivedAt = schedule.ConsentReceivedAt,
                SpecialNotes = schedule.SpecialNotes,
                HasRecord = schedule.Record != null,
                CreatedAt = schedule.CreatedAt
            };
        }

        private CheckupScheduleDetailResponseDTO MapToDetailResponseDTO(CheckupSchedule schedule)
        {
            var baseDto = MapToResponseDTO(schedule);

            return new CheckupScheduleDetailResponseDTO
            {
                Id = baseDto.Id,
                CampaignId = baseDto.CampaignId,
                CampaignName = baseDto.CampaignName,
                StudentId = baseDto.StudentId,
                StudentName = baseDto.StudentName,
                StudentCode = baseDto.StudentCode,
                Grade = baseDto.Grade,
                Section = baseDto.Section,
                NotifiedAt = baseDto.NotifiedAt,
                ScheduledAt = baseDto.ScheduledAt,
                ParentConsentStatus = baseDto.ParentConsentStatus,
                ConsentReceivedAt = baseDto.ConsentReceivedAt,
                SpecialNotes = baseDto.SpecialNotes,
                HasRecord = baseDto.HasRecord,
                CreatedAt = baseDto.CreatedAt,
                Student = schedule.Student != null ? new StudentBasicInfoDTO
                {
                    Id = schedule.Student.Id,
                    StudentCode = schedule.Student.StudentCode,
                    FirstName = schedule.Student.FirstName,
                    LastName = schedule.Student.LastName,
                    FullName = schedule.Student.FullName,
                    DateOfBirth = schedule.Student.DateOfBirth,
                    Grade = schedule.Student.Grade,
                    Section = schedule.Student.Section,
                    Image = schedule.Student.Image
                } : new StudentBasicInfoDTO(),
                Campaign = schedule.Campaign != null ? new CheckupCampaignBasicInfoDTO
                {
                    Id = schedule.Campaign.Id,
                    Name = schedule.Campaign.Name,
                    SchoolYear = schedule.Campaign.SchoolYear,
                    Status = schedule.Campaign.Status
                } : new CheckupCampaignBasicInfoDTO(),
                Record = schedule.Record != null ? new CheckupRecordBasicInfoDTO
                {
                    Id = schedule.Record.Id,
                    Status = schedule.Record.Status,
                    CheckupDate = schedule.Record.ExaminedAt,
                    Diagnosis = schedule.Record.Remarks,
                    Recommendations = schedule.Record.Remarks
                } : null
            };
        }

        #endregion
    }
}