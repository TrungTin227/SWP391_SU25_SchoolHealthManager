namespace Services.Interfaces
{
    public interface IVaccinationCampaignService
    {
        // Basic CRUD Operations
        Task<ApiResult<PagedList<VaccinationCampaignResponseDTO>>> GetVaccinationCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            VaccinationCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<ApiResult<VaccinationCampaignResponseDTO>> GetVaccinationCampaignByIdAsync(Guid id);

        Task<ApiResult<VaccinationCampaignDetailResponseDTO>> GetVaccinationCampaignDetailByIdAsync(Guid id);

        Task<ApiResult<VaccinationCampaignResponseDTO>> CreateVaccinationCampaignAsync(CreateVaccinationCampaignRequest request);

        Task<ApiResult<VaccinationCampaignResponseDTO>> UpdateVaccinationCampaignAsync(UpdateVaccinationCampaignRequest request);

        // Status Management (theo workflow Pending → InProgress → Resolved)
        Task<ApiResult<VaccinationCampaignResponseDTO>> StartCampaignAsync(Guid campaignId, string? notes = null);

        Task<ApiResult<VaccinationCampaignResponseDTO>> CompleteCampaignAsync(Guid campaignId, string? notes = null);

        Task<ApiResult<VaccinationCampaignResponseDTO>> CancelCampaignAsync(Guid campaignId, string? notes = null);

        // Batch Operations
        Task<ApiResult<BatchOperationResultDTO>> SoftDeleteCampaignsAsync(List<Guid> campaignIds);

        Task<ApiResult<BatchOperationResultDTO>> RestoreCampaignsAsync(List<Guid> campaignIds);

        Task<ApiResult<BatchOperationResultDTO>> BatchUpdateCampaignStatusAsync(BatchUpdateCampaignStatusRequest request);

        // Soft Delete Management
        Task<ApiResult<PagedList<VaccinationCampaignResponseDTO>>> GetSoftDeletedCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
    }
}