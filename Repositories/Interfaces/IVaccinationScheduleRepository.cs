namespace Repositories.Interfaces
{
    public interface IVaccinationScheduleRepository : IGenericRepository<VaccinationSchedule, Guid>
    {
        // Business queries
        Task<PagedList<VaccinationSchedule>> GetSchedulesAsync(
            Guid? campaignId,
            DateTime? startDate,
            DateTime? endDate,
            ScheduleStatus? status,
            string? searchTerm,
            int pageNumber,
            int pageSize);
        Task<PagedList<VaccinationSchedule>> GetSchedulesByCampaignAsync(
            Guid campaignId, int pageNumber, int pageSize, string? searchTerm = null);

        Task<PagedList<VaccinationSchedule>> GetSchedulesByDateRangeAsync(
            DateTime startDate, DateTime endDate, int pageNumber, int pageSize, string? searchTerm = null);

        Task<List<VaccinationSchedule>> GetSchedulesByStatusAsync(ScheduleStatus status);

        Task<VaccinationSchedule?> GetScheduleWithDetailsAsync(Guid id);

        Task<List<VaccinationSchedule>> GetSchedulesByIdsAsync(List<Guid> ids, bool includeDeleted = false);

        // Student management
        Task<bool> AddStudentsToScheduleAsync(Guid scheduleId, List<Guid> studentIds, Guid addedBy);

        Task<bool> RemoveStudentsFromScheduleAsync(Guid scheduleId, List<Guid> studentIds);

        Task<List<SessionStudent>> GetSessionStudentsByScheduleAsync(Guid scheduleId);

        // Status management
        Task<bool> UpdateScheduleStatusAsync(Guid scheduleId, ScheduleStatus newStatus, Guid updatedBy);

        Task<bool> BatchUpdateScheduleStatusAsync(List<Guid> scheduleIds, ScheduleStatus newStatus, Guid updatedBy);

        // Soft delete operations
        Task<PagedList<VaccinationSchedule>> GetSoftDeletedSchedulesAsync(
            int pageNumber, int pageSize, string? searchTerm = null);

        Task<bool> RestoreScheduleAsync(Guid id, Guid restoredBy);

        Task<bool> BatchRestoreSchedulesAsync(List<Guid> ids, Guid restoredBy);

        // Validation
        Task<bool> IsScheduleConflictAsync(Guid campaignId, DateTime scheduledAt, Guid? excludeScheduleId = null);

        Task<bool> CanDeleteScheduleAsync(Guid id);
        Task<bool> HasStudentScheduleConflictAsync(Guid studentId, DateTime scheduledAt, Guid? excludeScheduleId = null);
    }
}