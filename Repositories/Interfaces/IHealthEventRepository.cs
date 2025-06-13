namespace Repositories.Interfaces
{
    public interface IHealthEventRepository : IGenericRepository<HealthEvent, Guid>
    {
        // Basic queries
        Task<PagedList<HealthEvent>> GetHealthEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            EventStatus? status = null, EventType? eventType = null,
            Guid? studentId = null, DateTime? fromDate = null, DateTime? toDate = null);

        Task<HealthEvent?> GetHealthEventWithDetailsAsync(Guid id);

        // Batch operations
        Task<List<HealthEvent>> GetHealthEventsByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<int> SoftDeleteHealthEventsAsync(List<Guid> ids, Guid deletedBy);
        Task<int> RestoreHealthEventsAsync(List<Guid> ids, Guid restoredBy);
        Task<int> PermanentDeleteHealthEventsAsync(List<Guid> ids);

        // Status management
        Task<bool> UpdateEventStatusAsync(Guid eventId, EventStatus newStatus, Guid updatedBy);
        // Soft delete queries
        Task<PagedList<HealthEvent>> GetSoftDeletedEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);

        // Statistics
        Task<Dictionary<EventStatus, int>> GetEventStatusStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<EventType, int>> GetEventTypeStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}