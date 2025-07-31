using BusinessObjects.Common;

namespace Services.Interfaces
{
    public interface IHealthEventService
    {
        // Basic CRUD Operations
        Task<ApiResult<PagedList<HealthEventResponseDTO>>> GetHealthEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            EventStatus? status = null, EventType? eventType = null,
            Guid? studentId = null, DateTime? fromDate = null, DateTime? toDate = null, bool filterByCurrentUser = false);

        Task<ApiResult<HealthEventDetailResponseDTO>> GetHealthEventByIdAsync(Guid id);
        Task<ApiResult<HealthEventResponseDTO>> CreateHealthEventAsync(CreateHealthEventRequestDTO request);

        // Batch Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteHealthEventsAsync(List<Guid> ids, bool isPermanent = false);
        Task<ApiResult<BatchOperationResultDTO>> RestoreHealthEventsAsync(List<Guid> ids);

        // Soft Delete Operations
        Task<ApiResult<PagedList<HealthEventResponseDTO>>> GetSoftDeletedEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);

        // Workflow Operations
        Task<ApiResult<HealthEventResponseDTO>> UpdateHealthEventWithTreatmentAsync(UpdateHealthEventRequest request);
        Task<ApiResult<HealthEventDetailResponseDTO>> ResolveHealthEventAsync(Guid id, ResolveHealthEventRequest request);

        // Statistics
        Task<ApiResult<HealthEventStatisticsResponseDTO>> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        Task<ApiResult<List<HealthEventDetailResponseDTO>>> GetHealthForParentAsync();
        Task<ApiResult<HealthEventResponseDTO>> TreatHealthEventAsync(TreatHealthEventRequest request);
        Task<ApiResult<bool>> RecordParentAckAsync(Guid eventId);
    }
}