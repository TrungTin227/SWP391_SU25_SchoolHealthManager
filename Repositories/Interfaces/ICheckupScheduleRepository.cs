namespace Repositories.Interfaces
{
    public interface ICheckupScheduleRepository : IGenericRepository<CheckupSchedule, Guid>
    {
        // Basic Operations
        Task<CheckupSchedule?> GetCheckupScheduleByIdAsync(Guid id);
        Task<List<CheckupSchedule>> GetCheckupSchedulesByStudentIdAsync(Guid studentId);
        Task<PagedList<CheckupSchedule>> GetCheckupSchedulesAsync(
            int pageNumber, int pageSize, Guid? campaignId = null,
            CheckupScheduleStatus? status = null, string? searchTerm = null);

        // Campaign Related
        Task<List<CheckupSchedule>> GetSchedulesByCampaignAsync(Guid campaignId);
        Task<int> GetScheduleCountByCampaignAsync(Guid campaignId);
        Task<int> GetCompletedScheduleCountByCampaignAsync(Guid campaignId);

        // Batch Operations
        Task<int> BatchCreateSchedulesAsync(List<CheckupSchedule> schedules);
        Task<int> BatchUpdateScheduleStatusAsync(List<Guid> scheduleIds, CheckupScheduleStatus status, Guid updatedBy);
        Task<int> BatchSoftDeleteAsync(List<Guid> scheduleIds, Guid deletedBy);
        Task<int> BatchRestoreAsync(List<Guid> scheduleIds, Guid updatedBy);

        // Validation
        Task<bool> HasConflictingScheduleAsync(Guid studentId, DateTime scheduledAt, Guid? excludeId = null);

        // Statistics
        Task<Dictionary<CheckupScheduleStatus, int>> GetScheduleStatusStatisticsAsync(Guid? campaignId = null);
    }
}