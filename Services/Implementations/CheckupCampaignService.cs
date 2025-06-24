using DTOs.CheckupCampaign.Request;
using DTOs.CheckupCampaign.Response;
using DTOs.CheckupSchedule.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class CheckupCampaignService : BaseService<CheckupCampaign, Guid>, ICheckupCampaignService
    {
        private readonly ILogger<CheckupCampaignService> _logger;

        public CheckupCampaignService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICurrentTime currentTime,
            ILogger<CheckupCampaignService> logger)
            : base(unitOfWork.CheckupCampaignRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger;
        }

        public async Task<ApiResult<PagedList<CheckupCampaignResponseDTO>>> GetCheckupCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            CheckupCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Sử dụng repository thông qua UnitOfWork
                var campaignsPaged = await _unitOfWork.CheckupCampaignRepository
                    .GetCheckupCampaignsAsync(pageNumber, pageSize, searchTerm, status, startDate, endDate);

                // Chuyển từng entity thành DTO
                var responseDTOs = new List<CheckupCampaignResponseDTO>();
                foreach (var campaign in campaignsPaged)
                {
                    var dto = await MapToResponseDTO(campaign);
                    responseDTOs.Add(dto);
                }

                // Tạo PagedList<DTO> mới, dùng MetaData từ campaignsPaged
                var result = new PagedList<CheckupCampaignResponseDTO>(
                    responseDTOs,
                    campaignsPaged.MetaData.TotalCount,
                    campaignsPaged.MetaData.CurrentPage,
                    campaignsPaged.MetaData.PageSize);

                return ApiResult<PagedList<CheckupCampaignResponseDTO>>.Success(
                    result,
                    "Lấy danh sách chiến dịch khám định kỳ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách chiến dịch khám định kỳ");
                return ApiResult<PagedList<CheckupCampaignResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<CheckupCampaignResponseDTO>> GetCheckupCampaignByIdAsync(Guid id)
        {
            try
            {
                var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(id);
                if (campaign == null)
                {
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy chiến dịch khám định kỳ với ID: {id}"));
                }

                var response = await MapToResponseDTO(campaign);
                return ApiResult<CheckupCampaignResponseDTO>.Success(response,
                    "Lấy thông tin chiến dịch khám định kỳ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin chiến dịch khám định kỳ với ID: {Id}", id);
                return ApiResult<CheckupCampaignResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<CheckupCampaignDetailResponseDTO>> GetCheckupCampaignDetailByIdAsync(Guid id)
        {
            try
            {
                var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignWithDetailsAsync(id);
                if (campaign == null)
                {
                    return ApiResult<CheckupCampaignDetailResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy chiến dịch khám định kỳ với ID: {id}"));
                }

                var response = await MapToDetailResponseDTO(campaign);
                return ApiResult<CheckupCampaignDetailResponseDTO>.Success(response,
                    "Lấy chi tiết chiến dịch khám định kỳ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết chiến dịch khám định kỳ với ID: {Id}", id);
                return ApiResult<CheckupCampaignDetailResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<CheckupCampaignResponseDTO>> CreateCheckupCampaignAsync(CreateCheckupCampaignRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate campaign name uniqueness
                    if (await _unitOfWork.CheckupCampaignRepository.CampaignNameExistsAsync(request.Name))
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new ArgumentException("Tên chiến dịch khám định kỳ đã tồn tại"));
                    }

                    // Validate date range
                    if (request.StartDate.HasValue && request.EndDate.HasValue &&
                        request.StartDate >= request.EndDate)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc"));
                    }

                    var campaign = MapFromCreateRequest(request);
                    campaign.Status = CheckupCampaignStatus.Planning; // Initial status

                    var createdCampaign = await CreateAsync(campaign);

                    // Create schedules for students
                    if (request.StudentIds.Any() || request.Grades.Any() || request.Sections.Any())
                    {
                        await CreateSchedulesForCampaign(createdCampaign.Id, request);
                    }

                    var response = await MapToResponseDTO(createdCampaign);

                    _logger.LogInformation("Chiến dịch khám định kỳ được tạo thành công: {CampaignId}", createdCampaign.Id);
                    return ApiResult<CheckupCampaignResponseDTO>.Success(response, "Tạo chiến dịch khám định kỳ thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo chiến dịch khám định kỳ");
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<CheckupCampaignResponseDTO>> UpdateCheckupCampaignAsync(UpdateCheckupCampaignRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(request.Id);
                    if (campaign == null)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch khám định kỳ"));
                    }

                    // Validate campaign name uniqueness if name is being updated
                    if (!string.IsNullOrEmpty(request.Name) && request.Name != campaign.Name)
                    {
                        if (await _unitOfWork.CheckupCampaignRepository.CampaignNameExistsAsync(request.Name, request.Id))
                        {
                            return ApiResult<CheckupCampaignResponseDTO>.Failure(
                                new ArgumentException("Tên chiến dịch khám định kỳ đã tồn tại"));
                        }
                    }

                    // Update campaign properties
                    UpdateFromRequest(campaign, request);

                    var updatedCampaign = await UpdateAsync(campaign);
                    var response = await MapToResponseDTO(updatedCampaign);

                    _logger.LogInformation("Chiến dịch khám định kỳ được cập nhật thành công: {CampaignId}", updatedCampaign.Id);
                    return ApiResult<CheckupCampaignResponseDTO>.Success(response, "Cập nhật chiến dịch khám định kỳ thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật chiến dịch khám định kỳ: {CampaignId}", request.Id);
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        #region Status Management

        public async Task<ApiResult<CheckupCampaignResponseDTO>> StartCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                    if (campaign == null)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch khám định kỳ"));
                    }

                    if (campaign.Status != CheckupCampaignStatus.Planning &&
                        campaign.Status != CheckupCampaignStatus.Scheduled)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new InvalidOperationException($"Không thể bắt đầu chiến dịch từ trạng thái {campaign.Status}"));
                    }

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    await _unitOfWork.CheckupCampaignRepository.UpdateCampaignStatusAsync(
                        campaignId, CheckupCampaignStatus.InProgress, currentUserId);

                    var updatedCampaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                    var response = await MapToResponseDTO(updatedCampaign!);

                    _logger.LogInformation("Chiến dịch khám định kỳ được bắt đầu: {CampaignId}", campaignId);
                    return ApiResult<CheckupCampaignResponseDTO>.Success(response, "Bắt đầu chiến dịch khám định kỳ thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi bắt đầu chiến dịch khám định kỳ: {CampaignId}", campaignId);
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<CheckupCampaignResponseDTO>> CompleteCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                    if (campaign == null)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch khám định kỳ"));
                    }

                    if (campaign.Status != CheckupCampaignStatus.InProgress)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new InvalidOperationException($"Không thể hoàn thành chiến dịch từ trạng thái {campaign.Status}"));
                    }

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    await _unitOfWork.CheckupCampaignRepository.UpdateCampaignStatusAsync(
                        campaignId, CheckupCampaignStatus.Completed, currentUserId);

                    var updatedCampaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                    var response = await MapToResponseDTO(updatedCampaign!);

                    _logger.LogInformation("Chiến dịch khám định kỳ được hoàn thành: {CampaignId}", campaignId);
                    return ApiResult<CheckupCampaignResponseDTO>.Success(response, "Hoàn thành chiến dịch khám định kỳ thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi hoàn thành chiến dịch khám định kỳ: {CampaignId}", campaignId);
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<CheckupCampaignResponseDTO>> CancelCampaignAsync(Guid campaignId, string? reason = null)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                    if (campaign == null)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch khám định kỳ"));
                    }

                    if (campaign.Status == CheckupCampaignStatus.Completed ||
                        campaign.Status == CheckupCampaignStatus.Cancelled)
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new InvalidOperationException($"Không thể hủy chiến dịch từ trạng thái {campaign.Status}"));
                    }

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    await _unitOfWork.CheckupCampaignRepository.UpdateCampaignStatusAsync(
                        campaignId, CheckupCampaignStatus.Cancelled, currentUserId);

                    var updatedCampaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                    var response = await MapToResponseDTO(updatedCampaign!);

                    _logger.LogInformation("Chiến dịch khám định kỳ được hủy: {CampaignId}, Lý do: {Reason}", campaignId, reason);
                    return ApiResult<CheckupCampaignResponseDTO>.Success(response, "Hủy chiến dịch khám định kỳ thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi hủy chiến dịch khám định kỳ: {CampaignId}", campaignId);
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> BatchUpdateCampaignStatusAsync(DTOs.CheckupCampaign.Request.BatchUpdateCampaignStatusRequestDTO request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = request.CampaignIds.Count
                    };

                    var validCampaignIds = new List<Guid>();

                    // Kiểm tra từng campaign trước khi update
                    foreach (var campaignId in request.CampaignIds)
                    {
                        try
                        {
                            var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                            if (campaign == null)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "NotFound",
                                    Details = "Không tìm thấy chiến dịch"
                                });
                                continue;
                            }

                            // Validate transition logic nếu cần
                            if (!IsValidStatusTransition(campaign.Status, request.Status))
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "InvalidStatusTransition",
                                    Details = $"Không thể chuyển từ {campaign.Status} sang {request.Status}"
                                });
                                continue;
                            }

                            validCampaignIds.Add(campaignId);
                            result.SuccessIds.Add(campaignId.ToString());
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = campaignId.ToString(),
                                Error = "ProcessingError",
                                Details = ex.Message
                            });
                        }
                    }

                    // Thực hiện update cho các campaign hợp lệ
                    if (validCampaignIds.Any())
                    {
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var updatedCount = await _unitOfWork.CheckupCampaignRepository.BatchUpdateCampaignStatusAsync(
                            validCampaignIds, request.Status, currentUserId);

                        result.SuccessCount = updatedCount;
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = result.IsCompleteSuccess
                        ? $"Đã cập nhật trạng thái cho tất cả {result.SuccessCount} chiến dịch"
                        : result.IsPartialSuccess
                            ? $"Cập nhật thành công {result.SuccessCount}/{result.TotalRequested} chiến dịch"
                            : "Không thể cập nhật chiến dịch nào";

                    _logger.LogInformation("Batch update status: {SuccessCount}/{TotalCount} campaigns",
                        result.SuccessCount, result.TotalRequested);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật trạng thái hàng loạt chiến dịch khám định kỳ");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<BatchOperationResultDTO>> BatchDeleteCampaignsAsync(BatchDeleteCampaignRequestDTO request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = request.Ids.Count
                    };

                    var validCampaignIds = new List<Guid>();

                    // Kiểm tra từng campaign trước khi xóa
                    foreach (var campaignId in request.Ids)
                    {
                        try
                        {
                            var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                            if (campaign == null)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "NotFound",
                                    Details = "Không tìm thấy chiến dịch"
                                });
                                continue;
                            }

                            if (campaign.IsDeleted)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "AlreadyDeleted",
                                    Details = "Chiến dịch đã được xóa trước đó"
                                });
                                continue;
                            }

                            // Kiểm tra logic business nếu cần (ví dụ: không cho xóa chiến dịch đang diễn ra)
                            if (campaign.Status == CheckupCampaignStatus.InProgress)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "CannotDeleteActiveSession",
                                    Details = "Không thể xóa chiến dịch đang diễn ra"
                                });
                                continue;
                            }

                            validCampaignIds.Add(campaignId);
                            result.SuccessIds.Add(campaignId.ToString());
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = campaignId.ToString(),
                                Error = "ProcessingError",
                                Details = ex.Message
                            });
                        }
                    }

                    // Thực hiện xóa mềm cho các campaign hợp lệ
                    if (validCampaignIds.Any())
                    {
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var deletedCount = await _unitOfWork.CheckupCampaignRepository.BatchSoftDeleteAsync(
                            validCampaignIds, currentUserId);

                        result.SuccessCount = deletedCount;
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = result.IsCompleteSuccess
                        ? $"Đã xóa tất cả {result.SuccessCount} chiến dịch"
                        : result.IsPartialSuccess
                            ? $"Xóa thành công {result.SuccessCount}/{result.TotalRequested} chiến dịch"
                            : "Không thể xóa chiến dịch nào";

                    _logger.LogInformation("Batch delete: {SuccessCount}/{TotalCount} campaigns, Reason: {Reason}",
                        result.SuccessCount, result.TotalRequested, request.Reason);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa hàng loạt chiến dịch khám định kỳ");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<BatchOperationResultDTO>> BatchRestoreCampaignsAsync(BatchRestoreCampaignRequestDTO request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = request.Ids.Count
                    };

                    var validCampaignIds = new List<Guid>();

                    // Kiểm tra từng campaign trước khi khôi phục
                    foreach (var campaignId in request.Ids)
                    {
                        try
                        {
                            // Lấy cả campaign đã xóa để kiểm tra
                            var campaign = await _unitOfWork.CheckupCampaignRepository
                                    .GetQueryable()
                                    .Where(c => c.Id == campaignId)
                                    .FirstOrDefaultAsync();

                            if (campaign == null)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "NotFound",
                                    Details = "Không tìm thấy chiến dịch"
                                });
                                continue;
                            }

                            if (!campaign.IsDeleted)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "NotDeleted",
                                    Details = "Chiến dịch chưa bị xóa"
                                });
                                continue;
                            }

                            validCampaignIds.Add(campaignId);
                            result.SuccessIds.Add(campaignId.ToString());
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = campaignId.ToString(),
                                Error = "ProcessingError",
                                Details = ex.Message
                            });
                        }
                    }

                    // Thực hiện khôi phục cho các campaign hợp lệ
                    if (validCampaignIds.Any())
                    {
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var restoredCount = await _unitOfWork.CheckupCampaignRepository.BatchRestoreAsync(
                            validCampaignIds, currentUserId);

                        result.SuccessCount = restoredCount;
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = result.IsCompleteSuccess
                        ? $"Đã khôi phục tất cả {result.SuccessCount} chiến dịch"
                        : result.IsPartialSuccess
                            ? $"Khôi phục thành công {result.SuccessCount}/{result.TotalRequested} chiến dịch"
                            : "Không thể khôi phục chiến dịch nào";

                    _logger.LogInformation("Batch restore: {SuccessCount}/{TotalCount} campaigns",
                        result.SuccessCount, result.TotalRequested);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi khôi phục hàng loạt chiến dịch khám định kỳ");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        // Helper method để validate status transition
        private bool IsValidStatusTransition(CheckupCampaignStatus currentStatus, CheckupCampaignStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (CheckupCampaignStatus.Planning, CheckupCampaignStatus.Scheduled) => true,
                (CheckupCampaignStatus.Planning, CheckupCampaignStatus.Cancelled) => true,
                (CheckupCampaignStatus.Scheduled, CheckupCampaignStatus.InProgress) => true,
                (CheckupCampaignStatus.Scheduled, CheckupCampaignStatus.Cancelled) => true,
                (CheckupCampaignStatus.InProgress, CheckupCampaignStatus.Completed) => true,
                (CheckupCampaignStatus.InProgress, CheckupCampaignStatus.Cancelled) => true,
                _ => false
            };
        }

        #endregion

        public async Task<ApiResult<Dictionary<CheckupCampaignStatus, int>>> GetCampaignStatusStatisticsAsync()
        {
            try
            {
                var statistics = await _unitOfWork.CheckupCampaignRepository.GetCampaignStatusCountsAsync();
                return ApiResult<Dictionary<CheckupCampaignStatus, int>>.Success(statistics,
                    "Lấy thống kê trạng thái chiến dịch thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê trạng thái chiến dịch khám định kỳ");
                return ApiResult<Dictionary<CheckupCampaignStatus, int>>.Failure(ex);
            }
        }

        #region Private Methods

        private async Task CreateSchedulesForCampaign(Guid campaignId, CreateCheckupCampaignRequest request)
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

            // Create schedules
            var schedules = new List<CheckupSchedule>();
            var baseScheduledTime = request.ScheduledDate.Date.AddHours(8); // Start at 8 AM
            var intervalMinutes = 15; // 15 minutes per student
            var currentTime = baseScheduledTime;

            foreach (var studentId in studentIds)
            {
                var schedule = new CheckupSchedule
                {
                    Id = Guid.NewGuid(),
                    CampaignId = campaignId,
                    StudentId = studentId,
                    NotifiedAt = _currentTime.GetVietnamTime(),
                    ScheduledAt = currentTime,
                    ParentConsentStatus = CheckupScheduleStatus.Pending,
                    CreatedAt = _currentTime.GetVietnamTime(),
                    UpdatedAt = _currentTime.GetVietnamTime(),
                    CreatedBy = _currentUserService.GetUserId() ?? Guid.Empty,
                    UpdatedBy = _currentUserService.GetUserId() ?? Guid.Empty
                };

                schedules.Add(schedule);
                currentTime = currentTime.AddMinutes(intervalMinutes);
            }

            if (schedules.Any())
            {
                await _unitOfWork.CheckupScheduleRepository.BatchCreateSchedulesAsync(schedules);
            }
        }

        private async Task<CheckupCampaignResponseDTO> MapToResponseDTO(CheckupCampaign campaign)
        {
            var totalSchedules = await _unitOfWork.CheckupScheduleRepository.GetScheduleCountByCampaignAsync(campaign.Id);
            var completedSchedules = await _unitOfWork.CheckupScheduleRepository.GetCompletedScheduleCountByCampaignAsync(campaign.Id);

            return new CheckupCampaignResponseDTO
            {
                Id = campaign.Id,
                Name = campaign.Name,
                SchoolYear = campaign.SchoolYear,
                ScheduledDate = campaign.ScheduledDate,
                Description = campaign.Description,
                Status = campaign.Status,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                TotalSchedules = totalSchedules,
                CompletedSchedules = completedSchedules,
                CreatedAt = campaign.CreatedAt,
                CreatedByName = "System" // TODO: Get actual user name
            };
        }

        private async Task<CheckupCampaignDetailResponseDTO> MapToDetailResponseDTO(CheckupCampaign campaign)
        {
            var baseDto = await MapToResponseDTO(campaign);

            var schedulesDTOs = campaign.Schedules.Select(s => new CheckupScheduleResponseDTO
            {
                Id = s.Id,
                CampaignId = s.CampaignId,
                CampaignName = campaign.Name,
                StudentId = s.StudentId,
                StudentName = s.Student?.FullName ?? "",
                StudentCode = s.Student?.StudentCode ?? "",
                Grade = s.Student?.Grade ?? "",
                Section = s.Student?.Section ?? "",
                NotifiedAt = s.NotifiedAt,
                ScheduledAt = s.ScheduledAt,
                ParentConsentStatus = s.ParentConsentStatus,
                ConsentReceivedAt = s.ConsentReceivedAt,
                SpecialNotes = s.SpecialNotes,
                HasRecord = s.Record != null,
                CreatedAt = s.CreatedAt
            }).ToList();

            return new CheckupCampaignDetailResponseDTO
            {
                Id = baseDto.Id,
                Name = baseDto.Name,
                SchoolYear = baseDto.SchoolYear,
                ScheduledDate = baseDto.ScheduledDate,
                Description = baseDto.Description,
                Status = baseDto.Status,
                StartDate = baseDto.StartDate,
                EndDate = baseDto.EndDate,
                TotalSchedules = baseDto.TotalSchedules,
                CompletedSchedules = baseDto.CompletedSchedules,
                CreatedAt = baseDto.CreatedAt,
                CreatedByName = baseDto.CreatedByName,
                Schedules = schedulesDTOs
            };
        }

        private CheckupCampaign MapFromCreateRequest(CreateCheckupCampaignRequest request)
        {
            return new CheckupCampaign
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                SchoolYear = request.SchoolYear,
                ScheduledDate = request.ScheduledDate,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = CheckupCampaignStatus.Planning
            };
        }

        private void UpdateFromRequest(CheckupCampaign campaign, UpdateCheckupCampaignRequest request)
        {
            if (!string.IsNullOrEmpty(request.Name))
                campaign.Name = request.Name;

            if (!string.IsNullOrEmpty(request.SchoolYear))
                campaign.SchoolYear = request.SchoolYear;

            if (request.ScheduledDate.HasValue)
                campaign.ScheduledDate = request.ScheduledDate.Value;

            if (!string.IsNullOrEmpty(request.Description))
                campaign.Description = request.Description;

            if (request.StartDate.HasValue)
                campaign.StartDate = request.StartDate;

            if (request.EndDate.HasValue)
                campaign.EndDate = request.EndDate;
        }

        #endregion
    }
}