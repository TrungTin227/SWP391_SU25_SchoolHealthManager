using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccinationCampaignService : BaseService<VaccinationCampaign, Guid>, IVaccinationCampaignService
    {
        private readonly ILogger<VaccinationCampaignService> _logger;

        public VaccinationCampaignService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccinationCampaignService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccinationCampaignRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<VaccinationCampaignResponseDTO>>> GetVaccinationCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            VaccinationCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var pagedCampaigns = await _unitOfWork.VaccinationCampaignRepository.GetVaccinationCampaignsAsync(
                    pageNumber, pageSize, searchTerm, status, startDate, endDate);

                var responseItems = pagedCampaigns.Select(VaccinationCampaignMapper.MapToResponseDTO).ToList();
                var pagedResult = VaccinationCampaignMapper.ToPagedResult(pagedCampaigns, responseItems);

                return ApiResult<PagedList<VaccinationCampaignResponseDTO>>.Success(
                    pagedResult,
                    $"Lấy danh sách chiến dịch tiêm chủng thành công. Tổng số: {pagedResult.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách chiến dịch tiêm chủng");
                return ApiResult<PagedList<VaccinationCampaignResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> GetVaccinationCampaignByIdAsync(Guid id)
        {
            try
            {
                var campaign = await _unitOfWork.VaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(id);
                if (campaign == null)
                {
                    return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy chiến dịch tiêm chủng với ID: {id}"));
                }

                var response = VaccinationCampaignMapper.MapToResponseDTO(campaign);
                return ApiResult<VaccinationCampaignResponseDTO>.Success(response, "Lấy thông tin chiến dịch tiêm chủng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin chiến dịch tiêm chủng với ID: {Id}", id);
                return ApiResult<VaccinationCampaignResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccinationCampaignDetailResponseDTO>> GetVaccinationCampaignDetailByIdAsync(Guid id)
        {
            try
            {
                var campaign = await _unitOfWork.VaccinationCampaignRepository.GetVaccinationCampaignWithDetailsAsync(id);
                if (campaign == null)
                {
                    return ApiResult<VaccinationCampaignDetailResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy chiến dịch tiêm chủng với ID: {id}"));
                }

                var response = VaccinationCampaignMapper.MapToDetailResponseDTO(campaign);
                return ApiResult<VaccinationCampaignDetailResponseDTO>.Success(response, "Lấy chi tiết chiến dịch tiêm chủng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết chiến dịch tiêm chủng với ID: {Id}", id);
                return ApiResult<VaccinationCampaignDetailResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> CreateVaccinationCampaignAsync(CreateVaccinationCampaignRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate campaign name uniqueness
                    if (await _unitOfWork.VaccinationCampaignRepository.CampaignNameExistsAsync(request.Name))
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new ArgumentException("Tên chiến dịch tiêm chủng đã tồn tại"));
                    }

                    // Validate date range
                    if (request.StartDate >= request.EndDate)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc"));
                    }

                    var campaign = VaccinationCampaignMapper.MapFromCreateRequest(request);
                    campaign.Status = VaccinationCampaignStatus.Pending; // Initial status

                    // BaseService sẽ tự động xử lý audit fields
                    var createdCampaign = await CreateAsync(campaign);
                    var response = VaccinationCampaignMapper.MapToResponseDTO(createdCampaign);

                    _logger.LogInformation("Chiến dịch tiêm chủng được tạo thành công: {CampaignId}", createdCampaign.Id);
                    return ApiResult<VaccinationCampaignResponseDTO>.Success(response, "Tạo chiến dịch tiêm chủng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo chiến dịch tiêm chủng");
                    return ApiResult<VaccinationCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> UpdateVaccinationCampaignAsync(UpdateVaccinationCampaignRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _unitOfWork.VaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(request.Id);
                    if (campaign == null)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch tiêm chủng"));
                    }

                    // Validate campaign name uniqueness if name is being updated
                    if (!string.IsNullOrEmpty(request.Name) && request.Name != campaign.Name)
                    {
                        if (await _unitOfWork.VaccinationCampaignRepository.CampaignNameExistsAsync(request.Name, request.Id))
                        {
                            return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                                new ArgumentException("Tên chiến dịch tiêm chủng đã tồn tại"));
                        }
                    }

                    // Update campaign properties
                    VaccinationCampaignMapper.UpdateFromRequest(campaign, request);

                    // BaseService sẽ tự động xử lý audit fields
                    var updatedCampaign = await UpdateAsync(campaign);
                    var response = VaccinationCampaignMapper.MapToResponseDTO(updatedCampaign);

                    _logger.LogInformation("Chiến dịch tiêm chủng được cập nhật thành công: {CampaignId}", updatedCampaign.Id);
                    return ApiResult<VaccinationCampaignResponseDTO>.Success(response, "Cập nhật chiến dịch tiêm chủng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật chiến dịch tiêm chủng: {CampaignId}", request.Id);
                    return ApiResult<VaccinationCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Status Management (Workflow: Pending → InProgress → Resolved)

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> StartCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await UpdateCampaignStatusAsync(campaignId, VaccinationCampaignStatus.InProgress,
                "bắt đầu", VaccinationCampaignStatus.Pending);
        }

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> CompleteCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await UpdateCampaignStatusAsync(campaignId, VaccinationCampaignStatus.Resolved,
                "hoàn thành", VaccinationCampaignStatus.InProgress);
        }

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> CancelCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _unitOfWork.VaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
                    if (campaign == null)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch tiêm chủng"));
                    }

                    if (campaign.Status == VaccinationCampaignStatus.Resolved)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new InvalidOperationException("Không thể hủy chiến dịch đã hoàn thành"));
                    }

                    campaign.Status = VaccinationCampaignStatus.Cancelled;

                    // BaseService sẽ tự động xử lý UpdatedAt và UpdatedBy
                    var updatedCampaign = await UpdateAsync(campaign);
                    var response = VaccinationCampaignMapper.MapToResponseDTO(updatedCampaign);

                    _logger.LogInformation("Chiến dịch tiêm chủng được hủy: {CampaignId}", campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Success(response, "Hủy chiến dịch tiêm chủng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi hủy chiến dịch tiêm chủng: {CampaignId}", campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> SoftDeleteCampaignsAsync(List<Guid> campaignIds)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchInput(campaignIds, "xóa");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    _logger.LogInformation("Starting batch soft delete for {Count} vaccination campaigns", campaignIds.Count);

                    var result = new BatchOperationResultDTO { TotalRequested = campaignIds.Count };

                    foreach (var campaignId in campaignIds)
                    {
                        try
                        {
                            // Sử dụng BaseService DeleteAsync cho soft delete
                            var deleteResult = await DeleteAsync(campaignId);
                            if (deleteResult)
                            {
                                result.SuccessCount++;
                                result.SuccessIds.Add(campaignId.ToString());
                            }
                            else
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = campaignId.ToString(),
                                    Error = "Xóa thất bại",
                                    Details = "Không thể xóa chiến dịch tiêm chủng"
                                });
                                result.FailureCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = campaignId.ToString(),
                                Error = "Lỗi hệ thống",
                                Details = ex.Message
                            });
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("xóa", result);

                    _logger.LogInformation("Batch soft delete completed: {SuccessCount}/{TotalCount} vaccination campaigns",
                        result.SuccessCount, campaignIds.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa mềm chiến dịch tiêm chủng");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreCampaignsAsync(List<Guid> campaignIds)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchInput(campaignIds, "khôi phục");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    _logger.LogInformation("Starting batch restore for {Count} vaccination campaigns", campaignIds.Count);

                    var currentUserId = GetCurrentUserIdOrThrow();
                    var restoredCount = await _unitOfWork.VaccinationCampaignRepository.RestoreCampaignsAsync(campaignIds, currentUserId);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = campaignIds.Count,
                        SuccessCount = restoredCount,
                        FailureCount = campaignIds.Count - restoredCount,
                        SuccessIds = campaignIds.Take(restoredCount).Select(id => id.ToString()).ToList()
                    };

                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

                    _logger.LogInformation("Batch restore completed: {SuccessCount}/{TotalCount} vaccination campaigns",
                        restoredCount, campaignIds.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi khôi phục chiến dịch tiêm chủng");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<BatchOperationResultDTO>> BatchUpdateCampaignStatusAsync(BatchUpdateCampaignStatusRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchStatusUpdate(request);
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    _logger.LogInformation("Starting batch status update for {Count} vaccination campaigns", request.Updates.Count);

                    var result = new BatchOperationResultDTO { TotalRequested = request.Updates.Count };

                    foreach (var update in request.Updates)
                    {
                        try
                        {
                            var campaign = await _unitOfWork.VaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(update.CampaignId);
                            if (campaign == null)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = update.CampaignId.ToString(),
                                    Error = "Không tìm thấy",
                                    Details = "Chiến dịch tiêm chủng không tồn tại"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            // Validate status transition
                            if (!IsValidStatusTransition(campaign.Status, update.Status))
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = update.CampaignId.ToString(),
                                    Error = "Trạng thái không hợp lệ",
                                    Details = $"Không thể chuyển từ {campaign.Status} sang {update.Status}"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            campaign.Status = update.Status;

                            // BaseService sẽ tự động xử lý UpdatedAt và UpdatedBy
                            await UpdateAsync(campaign);

                            result.SuccessCount++;
                            result.SuccessIds.Add(update.CampaignId.ToString());
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = update.CampaignId.ToString(),
                                Error = "Lỗi hệ thống",
                                Details = ex.Message
                            });
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("cập nhật trạng thái", result);

                    _logger.LogInformation("Batch status update completed: {SuccessCount}/{TotalCount} vaccination campaigns",
                        result.SuccessCount, request.Updates.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật trạng thái chiến dịch tiêm chủng hàng loạt");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Soft Delete Management

        public async Task<ApiResult<PagedList<VaccinationCampaignResponseDTO>>> GetSoftDeletedCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var pagedCampaigns = await _unitOfWork.VaccinationCampaignRepository.GetSoftDeletedCampaignsAsync(
                    pageNumber, pageSize, searchTerm);

                var responseItems = pagedCampaigns.Select(VaccinationCampaignMapper.MapToResponseDTO).ToList();
                var pagedResult = VaccinationCampaignMapper.ToPagedResult(pagedCampaigns, responseItems);

                return ApiResult<PagedList<VaccinationCampaignResponseDTO>>.Success(
                    pagedResult, "Lấy danh sách chiến dịch tiêm chủng đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách chiến dịch tiêm chủng đã xóa");
                return ApiResult<PagedList<VaccinationCampaignResponseDTO>>.Failure(ex);
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

        /// <summary>
        /// Helper method cho status update operations
        /// </summary>
        private async Task<ApiResult<VaccinationCampaignResponseDTO>> UpdateCampaignStatusAsync(
            Guid campaignId,
            VaccinationCampaignStatus newStatus,
            string actionName,
            VaccinationCampaignStatus validCurrentStatus)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _unitOfWork.VaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
                    if (campaign == null)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch tiêm chủng"));
                    }

                    if (campaign.Status != validCurrentStatus)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new InvalidOperationException($"Không thể {actionName} chiến dịch từ trạng thái {campaign.Status}"));
                    }

                    campaign.Status = newStatus;

                    // BaseService sẽ tự động xử lý UpdatedAt và UpdatedBy
                    var updatedCampaign = await UpdateAsync(campaign);
                    var response = VaccinationCampaignMapper.MapToResponseDTO(updatedCampaign);

                    _logger.LogInformation("Chiến dịch tiêm chủng được {ActionName}: {CampaignId}", actionName, campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Success(response, $"{char.ToUpper(actionName[0])}{actionName.Substring(1)} chiến dịch tiêm chủng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi {ActionName} chiến dịch tiêm chủng: {CampaignId}", actionName, campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        /// <summary>
        /// Validate batch input
        /// </summary>
        private static (bool isValid, string message) ValidateBatchInput(List<Guid> ids, string operation)
        {
            if (ids == null || !ids.Any())
            {
                return (false, "Danh sách ID không được rỗng");
            }

            if (ids.Any(id => id == Guid.Empty))
            {
                return (false, "Danh sách chứa ID không hợp lệ");
            }

            if (ids.Count > 100)
            {
                return (false, $"Không thể {operation} quá 100 chiến dịch cùng lúc");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate batch status update request
        /// </summary>
        private static (bool isValid, string message) ValidateBatchStatusUpdate(BatchUpdateCampaignStatusRequest request)
        {
            if (request?.Updates == null || !request.Updates.Any())
            {
                return (false, "Danh sách cập nhật không được rỗng");
            }

            if (request.Updates.Any(u => u.CampaignId == Guid.Empty))
            {
                return (false, "Danh sách chứa ID không hợp lệ");
            }

            if (request.Updates.Count > 100)
            {
                return (false, "Không thể cập nhật quá 100 chiến dịch cùng lúc");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate status transition
        /// </summary>
        private static bool IsValidStatusTransition(VaccinationCampaignStatus currentStatus, VaccinationCampaignStatus newStatus)
        {
            return currentStatus switch
            {
                VaccinationCampaignStatus.Pending => newStatus is VaccinationCampaignStatus.InProgress or VaccinationCampaignStatus.Cancelled,
                VaccinationCampaignStatus.InProgress => newStatus is VaccinationCampaignStatus.Resolved or VaccinationCampaignStatus.Cancelled,
                VaccinationCampaignStatus.Resolved => false, // Cannot change from resolved
                VaccinationCampaignStatus.Cancelled => newStatus == VaccinationCampaignStatus.Pending, // Can restart cancelled
                _ => false
            };
        }

        /// <summary>
        /// Generate batch operation message
        /// </summary>
        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công tất cả {result.TotalRequested} chiến dịch tiêm chủng";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} chiến dịch tiêm chủng. " +
                       $"Không thể {operation} {result.FailureCount} chiến dịch";

            return $"Không thể {operation} bất kỳ chiến dịch tiêm chủng nào";
        }

        #endregion
    }
}