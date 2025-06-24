namespace Repositories.Interfaces
{
    public interface ICheckupCampaignRepository : IGenericRepository<CheckupCampaign, Guid>
    {
        Task<CheckupCampaign?> GetCheckupCampaignByIdAsync(Guid id);
        Task<CheckupCampaign?> GetCheckupCampaignWithDetailsAsync(Guid id);
        Task<PagedList<CheckupCampaign>> GetCheckupCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            CheckupCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<bool> CampaignNameExistsAsync(string name, Guid? excludeId = null);
        Task<int> UpdateCampaignStatusAsync(Guid campaignId, CheckupCampaignStatus status, Guid updatedBy);
        Task<int> BatchUpdateCampaignStatusAsync(List<Guid> campaignIds, CheckupCampaignStatus status, Guid updatedBy);
        Task<int> BatchSoftDeleteAsync(List<Guid> campaignIds, Guid deletedBy);
        Task<int> BatchRestoreAsync(List<Guid> campaignIds, Guid restoredBy);

        // Statistics
        Task<Dictionary<CheckupCampaignStatus, int>> GetCampaignStatusCountsAsync();
    }
}