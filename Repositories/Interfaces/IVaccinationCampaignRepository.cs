namespace Repositories.Interfaces
{
    public interface IVaccinationCampaignRepository : IGenericRepository<VaccinationCampaign, Guid>
    {
        Task<PagedList<VaccinationCampaign>> GetVaccinationCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            VaccinationCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<VaccinationCampaign?> GetVaccinationCampaignByIdAsync(Guid id);

        Task<VaccinationCampaign?> GetVaccinationCampaignWithDetailsAsync(Guid id);

        Task<List<VaccinationCampaign>> GetVaccinationCampaignsByIdsAsync(List<Guid> ids, bool includeDeleted = false);

        Task<PagedList<VaccinationCampaign>> GetSoftDeletedCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);

        Task<int> SoftDeleteCampaignsAsync(List<Guid> ids, Guid deletedBy);

        Task<int> RestoreCampaignsAsync(List<Guid> ids, Guid restoredBy);

        Task<bool> CampaignNameExistsAsync(string name, Guid? excludeId = null);

        Task<int> UpdateCampaignStatusAsync(Guid campaignId, VaccinationCampaignStatus status, Guid updatedBy);

        Task<int> BatchUpdateCampaignStatusAsync(List<Guid> campaignIds, VaccinationCampaignStatus status, Guid updatedBy);
    }
}