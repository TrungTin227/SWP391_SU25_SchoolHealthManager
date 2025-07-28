namespace Services.Interfaces
{
    public interface ICheckupCampaignService
    {
        // Basic CRUD Operations
        Task<ApiResult<PagedList<CheckupCampaignResponseDTO>>> GetCheckupCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            CheckupCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<ApiResult<CheckupCampaignResponseDTO>> GetCheckupCampaignByIdAsync(Guid id);
        Task<ApiResult<CheckupCampaignDetailResponseDTO>> GetCheckupCampaignDetailByIdAsync(Guid id);
        Task<ApiResult<CheckupCampaignResponseDTO>> CreateCheckupCampaignAsync(CreateCheckupCampaignRequest request);
        Task<ApiResult<CheckupCampaignResponseDTO>> UpdateCheckupCampaignAsync(UpdateCheckupCampaignRequest request);

        // Status Management
        Task<ApiResult<CheckupCampaignResponseDTO>> StartCampaignAsync(Guid campaignId, string? notes = null);
        Task<ApiResult<CheckupCampaignResponseDTO>> CompleteCampaignAsync(Guid campaignId, string? notes = null);
        Task<ApiResult<CheckupCampaignResponseDTO>> CancelCampaignAsync(Guid campaignId, string? reason = null);

        // Batch Operations - Sử dụng Common DTOs
        Task<ApiResult<BatchOperationResultDTO>> BatchUpdateCampaignStatusAsync(DTOs.CheckupCampaign.Request.BatchUpdateCampaignStatusRequestDTO request);
        Task<ApiResult<BatchOperationResultDTO>> BatchDeleteCampaignsAsync(BatchDeleteCampaignRequestDTO request);
        Task<ApiResult<BatchOperationResultDTO>> BatchRestoreCampaignsAsync(BatchRestoreCampaignRequestDTO request);

        // Statistics
        Task<ApiResult<Dictionary<CheckupCampaignStatus, int>>> GetCampaignStatusStatisticsAsync();

        Task<ApiResult<PagedList<CheckupCampaignResponseDTO>>> GetSoftDeletedCampaignsAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}