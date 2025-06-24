namespace Repositories.Interfaces
{
    public interface ICheckupScheduleRepository : IGenericRepository<CheckupSchedule, Guid>
    {
        Task<CheckupSchedule?> GetCheckupScheduleByIdAsync(Guid id);
        Task<PagedList<CheckupSchedule>> GetCheckupSchedulesAsync(
            int pageNumber, int pageSize, Guid? campaignId = null,
            CheckupScheduleStatus? status = null, string? searchTerm = null);

        Task<List<CheckupSchedule>> GetSchedulesByCampaignAsync(Guid campaignId);
        Task<int> BatchCreateSchedulesAsync(List<CheckupSchedule> schedules);
        Task<int> BatchUpdateScheduleStatusAsync(List<Guid> scheduleIds, CheckupScheduleStatus status, Guid updatedBy);
        Task<int> GetScheduleCountByCampaignAsync(Guid campaignId);
        Task<int> GetCompletedScheduleCountByCampaignAsync(Guid campaignId);
    }
}