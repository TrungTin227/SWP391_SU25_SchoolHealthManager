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

        #region CRUD Operations

        public async Task<ApiResult<PagedList<CheckupCampaignResponseDTO>>> GetCheckupCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            CheckupCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var campaignsPaged = await _unitOfWork.CheckupCampaignRepository
                    .GetCheckupCampaignsAsync(pageNumber, pageSize, searchTerm, status, startDate, endDate);

                var responseDTOs = new List<CheckupCampaignResponseDTO>();
                foreach (var campaign in campaignsPaged)
                {
                    var dto = await MapToResponseDTO(campaign);
                    responseDTOs.Add(dto);
                }

                var result = new PagedList<CheckupCampaignResponseDTO>(
                    responseDTOs,
                    campaignsPaged.MetaData.TotalCount,
                    campaignsPaged.MetaData.CurrentPage,
                    campaignsPaged.MetaData.PageSize);

                return ApiResult<PagedList<CheckupCampaignResponseDTO>>.Success(
                    result, "Lấy danh sách chiến dịch khám định kỳ thành công");
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

                    // BaseService sẽ tự động xử lý audit fields
                    var createdCampaign = await CreateAsync(campaign);

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

                    // BaseService sẽ tự động xử lý audit fields
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

        #endregion

        #region Status Management

        public async Task<ApiResult<CheckupCampaignResponseDTO>> StartCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await UpdateCampaignStatusAsync(campaignId, CheckupCampaignStatus.InProgress,
                "bắt đầu", CheckupCampaignStatus.Planning, CheckupCampaignStatus.Scheduled);
        }

        public async Task<ApiResult<CheckupCampaignResponseDTO>> CompleteCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var campaign = await _unitOfWork.CheckupCampaignRepository.GetCheckupCampaignByIdAsync(campaignId);
                if (campaign == null)
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy chiến dịch khám định kỳ"));

                // Không cho hoàn thành nếu chưa tới EndDate
                if (_currentTime.GetVietnamTime() < campaign.EndDate)
                    return ApiResult<CheckupCampaignResponseDTO>.Failure(
                        new InvalidOperationException($"Chưa tới ngày kết thúc ({campaign.EndDate:dd/MM/yyyy}). Không thể hoàn thành."));

                // Re-use logic cũ
                return await UpdateCampaignStatusAsync(campaignId, CheckupCampaignStatus.Completed,
                        "hoàn thành", CheckupCampaignStatus.InProgress);
            });
        }

        public async Task<ApiResult<CheckupCampaignResponseDTO>> CancelCampaignAsync(Guid campaignId, string? reason = null)
        {
            return await UpdateCampaignStatusAsync(campaignId, CheckupCampaignStatus.Cancelled,
                "hủy", CheckupCampaignStatus.Planning, CheckupCampaignStatus.Scheduled, CheckupCampaignStatus.InProgress);
        }

        private async Task<ApiResult<CheckupCampaignResponseDTO>> UpdateCampaignStatusAsync(
            Guid campaignId,
            CheckupCampaignStatus newStatus,
            string actionName,
            params CheckupCampaignStatus[] validCurrentStatuses)
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

                    if (!validCurrentStatuses.Contains(campaign.Status))
                    {
                        return ApiResult<CheckupCampaignResponseDTO>.Failure(
                            new InvalidOperationException($"Không thể {actionName} chiến dịch từ trạng thái {campaign.Status}"));
                    }

                    campaign.Status = newStatus;

                    // BaseService sẽ tự động xử lý UpdatedAt và UpdatedBy
                    var updatedCampaign = await UpdateAsync(campaign);
                    var response = await MapToResponseDTO(updatedCampaign);

                    _logger.LogInformation("Chiến dịch khám định kỳ được {ActionName}: {CampaignId}", actionName, campaignId);
                    return ApiResult<CheckupCampaignResponseDTO>.Success(response, $"{char.ToUpper(actionName[0])}{actionName.Substring(1)} chiến dịch khám định kỳ thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi {ActionName} chiến dịch khám định kỳ: {CampaignId}", actionName, campaignId);
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
                        var currentUserId = GetCurrentUserIdOrThrow();
                        var updatedCount = await _unitOfWork.CheckupCampaignRepository.BatchUpdateCampaignStatusAsync(
                            validCampaignIds, request.Status, currentUserId);

                        result.SuccessCount = updatedCount;
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage("cập nhật trạng thái", result);

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
                                result.FailureCount++;
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
                                result.FailureCount++;
                                continue;
                            }

                            if (campaign.Status == CheckupCampaignStatus.InProgress)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "CannotDeleteActiveSession",
                                    Details = "Không thể xóa chiến dịch đang diễn ra"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            // Sử dụng BaseService DeleteAsync
                            var deleteResult = await DeleteAsync(campaignId);
                            if (deleteResult)
                            {
                                result.SuccessIds.Add(campaignId.ToString());
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "DeleteFailed",
                                    Details = "Không thể xóa chiến dịch"
                                });
                                result.FailureCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = campaignId.ToString(),
                                Error = "ProcessingError",
                                Details = ex.Message
                            });
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("xóa", result);

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

                    foreach (var campaignId in request.Ids)
                    {
                        try
                        {
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

                    if (validCampaignIds.Any())
                    {
                        var currentUserId = GetCurrentUserIdOrThrow();
                        var restoredCount = await _unitOfWork.CheckupCampaignRepository.BatchRestoreAsync(
                            validCampaignIds, currentUserId);

                        result.SuccessCount = restoredCount;
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

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

        #endregion

        #region Statistics

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

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Lấy current user ID và ném exception nếu null
        /// </summary>
        private Guid GetCurrentUserIdOrThrow()
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng hiện tại");
            }
            return currentUserId.Value;
        }

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

        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công {result.SuccessCount} chiến dịch";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} chiến dịch. " +
                       $"{result.FailureCount} thất bại";

            return $"Không thể {operation} chiến dịch nào. Tất cả {result.FailureCount} yêu cầu đều thất bại";
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
                // Không cần set Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy - BaseService sẽ xử lý
                Name = request.Name,
                SchoolYear = request.SchoolYear,
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

            if (!string.IsNullOrEmpty(request.Description))
                campaign.Description = request.Description;

            if (request.StartDate.HasValue)
                campaign.StartDate = request.StartDate;

            if (request.EndDate.HasValue)
                campaign.EndDate = request.EndDate;
        }

        #endregion
        public async Task<ApiResult<PagedList<CheckupCampaignResponseDTO>>> GetSoftDeletedCampaignsAsync(
    int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var paged = await _unitOfWork.CheckupCampaignRepository
                    .GetSoftDeletedCampaignsAsync(pageNumber, pageSize, searchTerm);

                var dtos = new List<CheckupCampaignResponseDTO>();
                foreach (var c in paged)
                    dtos.Add(await MapToResponseDTO(c));

                var result = new PagedList<CheckupCampaignResponseDTO>(
                    dtos, paged.MetaData.TotalCount,
                    paged.MetaData.CurrentPage, paged.MetaData.PageSize);

                return ApiResult<PagedList<CheckupCampaignResponseDTO>>.Success(
                    result, "Lấy danh sách chiến dịch đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách chiến dịch đã xóa");
                return ApiResult<PagedList<CheckupCampaignResponseDTO>>.Failure(ex);
            }
        }
    }
}