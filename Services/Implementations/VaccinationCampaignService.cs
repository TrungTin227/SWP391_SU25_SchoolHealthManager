using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccinationCampaignService : BaseService<VaccinationCampaign, Guid>, IVaccinationCampaignService
    {
        private readonly IVaccinationCampaignRepository _vaccinationCampaignRepository;
        private readonly IVaccineTypeRepository _vaccineTypeRepository;
        private readonly ILogger<VaccinationCampaignService> _logger;

        public VaccinationCampaignService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccinationCampaignService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccinationCampaignRepository, currentUserService, unitOfWork, currentTime)
        {
            _vaccinationCampaignRepository = unitOfWork.VaccinationCampaignRepository;
            _vaccineTypeRepository = unitOfWork.VaccineTypeRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<VaccinationCampaignResponseDTO>>> GetVaccinationCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            VaccinationCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var pagedCampaigns = await _vaccinationCampaignRepository.GetVaccinationCampaignsAsync(
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
                var campaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(id);
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
                var campaign = await _vaccinationCampaignRepository.GetVaccinationCampaignWithDetailsAsync(id);
                if (campaign == null)
                {
                    return ApiResult<VaccinationCampaignDetailResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy chiến dịch tiêm chủng với ID: {id}"));
                }

                var response = VaccinationCampaignMapper.MapToDetailResponseDTO(campaign); //  MAP VACCINE TYPE
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
                    if (await _vaccinationCampaignRepository.CampaignNameExistsAsync(request.Name))
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
                    var campaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(request.Id);
                    if (campaign == null)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch tiêm chủng"));
                    }

                    // Validate campaign name uniqueness if name is being updated
                    if (!string.IsNullOrEmpty(request.Name) && request.Name != campaign.Name)
                    {
                        if (await _vaccinationCampaignRepository.CampaignNameExistsAsync(request.Name, request.Id))
                        {
                            return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                                new ArgumentException("Tên chiến dịch tiêm chủng đã tồn tại"));
                        }
                    }

                    // Update campaign properties
                    VaccinationCampaignMapper.UpdateFromRequest(campaign, request);

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
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
                    if (campaign == null)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch tiêm chủng"));
                    }

                    if (campaign.Status != VaccinationCampaignStatus.Pending)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new InvalidOperationException($"Không thể bắt đầu chiến dịch từ trạng thái {campaign.Status}"));
                    }

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    await _vaccinationCampaignRepository.UpdateCampaignStatusAsync(
                        campaignId, VaccinationCampaignStatus.InProgress, currentUserId);

                    var updatedCampaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
                    var response = VaccinationCampaignMapper.MapToResponseDTO(updatedCampaign!);

                    _logger.LogInformation("Chiến dịch tiêm chủng được bắt đầu: {CampaignId}", campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Success(response, "Bắt đầu chiến dịch tiêm chủng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi bắt đầu chiến dịch tiêm chủng: {CampaignId}", campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> CompleteCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
                    if (campaign == null)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy chiến dịch tiêm chủng"));
                    }

                    if (campaign.Status != VaccinationCampaignStatus.InProgress)
                    {
                        return ApiResult<VaccinationCampaignResponseDTO>.Failure(
                            new InvalidOperationException($"Không thể hoàn thành chiến dịch từ trạng thái {campaign.Status}"));
                    }

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    await _vaccinationCampaignRepository.UpdateCampaignStatusAsync(
                        campaignId, VaccinationCampaignStatus.Resolved, currentUserId);

                    var updatedCampaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
                    var response = VaccinationCampaignMapper.MapToResponseDTO(updatedCampaign!);

                    _logger.LogInformation("Chiến dịch tiêm chủng được hoàn thành: {CampaignId}", campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Success(response, "Hoàn thành chiến dịch tiêm chủng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi hoàn thành chiến dịch tiêm chủng: {CampaignId}", campaignId);
                    return ApiResult<VaccinationCampaignResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccinationCampaignResponseDTO>> CancelCampaignAsync(Guid campaignId, string? notes = null)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var campaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
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

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    await _vaccinationCampaignRepository.UpdateCampaignStatusAsync(
                        campaignId, VaccinationCampaignStatus.Cancelled, currentUserId);

                    var updatedCampaign = await _vaccinationCampaignRepository.GetVaccinationCampaignByIdAsync(campaignId);
                    var response = VaccinationCampaignMapper.MapToResponseDTO(updatedCampaign!);

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
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var deletedCount = await _vaccinationCampaignRepository.SoftDeleteCampaignsAsync(campaignIds, currentUserId);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = campaignIds.Count,
                        SuccessCount = deletedCount,
                        FailureCount = campaignIds.Count - deletedCount
                    };

                    var message = GenerateBatchOperationMessage("xóa", result);
                    _logger.LogInformation("Xóa mềm chiến dịch tiêm chủng: {SuccessCount}/{TotalCount}", deletedCount, campaignIds.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, message);
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
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var restoredCount = await _vaccinationCampaignRepository.RestoreCampaignsAsync(campaignIds, currentUserId);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = campaignIds.Count,
                        SuccessCount = restoredCount,
                        FailureCount = campaignIds.Count - restoredCount
                    };

                    var message = GenerateBatchOperationMessage("khôi phục", result);
                    _logger.LogInformation("Khôi phục chiến dịch tiêm chủng: {SuccessCount}/{TotalCount}", restoredCount, campaignIds.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, message);
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
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var totalSuccess = 0;
                    var successIds = new List<string>();
                    var errors = new List<BatchOperationErrorDTO>(); // Thay đổi từ List<string> thành List<BatchOperationErrorDTO>

                    foreach (var update in request.Updates)
                    {
                        try
                        {
                            var affected = await _vaccinationCampaignRepository.UpdateCampaignStatusAsync(
                                update.CampaignId, update.Status, currentUserId);

                            if (affected > 0)
                            {
                                totalSuccess++;
                                successIds.Add(update.CampaignId.ToString()); // Thêm ID vào danh sách thành công
                            }
                            else
                            {
                                errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = update.CampaignId.ToString(),
                                    Details = $"Không thể cập nhật trạng thái cho chiến dịch {update.CampaignId}",
                                    Error = "UPDATE_FAILED"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new BatchOperationErrorDTO
                            {
                                Id = update.CampaignId.ToString(),
                                Details = $"Lỗi khi cập nhật chiến dịch {update.CampaignId}: {ex.Message}",
                                Error = "EXCEPTION"
                            });
                        }
                    }

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = request.Updates.Count,
                        SuccessCount = totalSuccess,
                        FailureCount = request.Updates.Count - totalSuccess,
                        SuccessIds = successIds, // Gán danh sách ID thành công
                        Errors = errors // Bây giờ đã đúng kiểu dữ liệu
                    };

                    var message = GenerateBatchOperationMessage("cập nhật trạng thái", result);
                    result.Message = message;

                    _logger.LogInformation("Cập nhật trạng thái chiến dịch tiêm chủng: {SuccessCount}/{TotalCount}", totalSuccess, request.Updates.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, message);
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
                var pagedCampaigns = await _vaccinationCampaignRepository.GetSoftDeletedCampaignsAsync(
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

        #region Helper Methods

        private string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
            {
                return $"Đã {operation} thành công tất cả {result.TotalRequested} chiến dịch tiêm chủng";
            }
            else if (result.IsCompleteFailure)
            {
                return $"Không thể {operation} bất kỳ chiến dịch tiêm chủng nào";
            }
            else if (result.IsPartialSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} chiến dịch tiêm chủng. " +
                       $"Không thể {operation} {result.FailureCount} chiến dịch";
            }

            return $"Hoàn thành việc {operation} chiến dịch tiêm chủng";
        }

        #endregion
    }
}