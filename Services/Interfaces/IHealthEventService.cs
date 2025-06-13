using DTOs.HealthEventDTOs.Request;
using DTOs.HealthEventDTOs.Response;

namespace Services.Interfaces
{
    public interface IHealthEventService
    {
        // Basic CRUD Operations
        Task<ApiResult<PagedList<HealthEventResponseDTO>>> GetHealthEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            EventStatus? status = null, EventType? eventType = null,
            Guid? studentId = null, DateTime? fromDate = null, DateTime? toDate = null);

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
        Task<ApiResult<HealthEventResponseDTO>> ResolveHealthEventAsync(ResolveHealthEventRequest request);

        // Statistics
        Task<ApiResult<HealthEventStatisticsResponseDTO>> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}