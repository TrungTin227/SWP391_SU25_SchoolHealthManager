namespace Services.Interfaces
{
    public interface IVaccinationScheduleService
    {
        // CRUD Operations
        Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSchedulesAsync(
            Guid? campaignId,
            DateTime? startDate,
            DateTime? endDate,
            ScheduleStatus? status,
            string? searchTerm,
            int pageNumber,
            int pageSize);

        Task<ApiResult<List<VaccinationScheduleResponseDTO>>> CreateSchedulesAsync(CreateVaccinationScheduleRequest request);

        Task<ApiResult<VaccinationScheduleDetailResponseDTO>> UpdateScheduleAsync(Guid id, UpdateVaccinationScheduleRequest request);
        Task<ApiResult<VaccinationScheduleDetailResponseDTO>> GetScheduleByIdAsync(Guid id);
        Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSchedulesByCampaignAsync(
            Guid campaignId, int pageNumber, int pageSize, string? searchTerm = null);
        Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSchedulesByDateRangeAsync(
            DateTime startDate, DateTime endDate, int pageNumber, int pageSize, string? searchTerm = null);

        // Student Management
        Task<ApiResult<bool>> AddStudentsToScheduleAsync(Guid scheduleId, List<Guid> studentIds);
        Task<ApiResult<bool>> RemoveStudentsFromScheduleAsync(Guid scheduleId, List<Guid> studentIds);

        // Status Management
        Task<ApiResult<bool>> UpdateScheduleStatusAsync(Guid scheduleId, ScheduleStatus newStatus);
        Task<ApiResult<BatchOperationResultDTO>> BatchUpdateScheduleStatusAsync(BatchUpdateScheduleStatusRequest request);

        // Batch Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteSchedulesAsync(List<Guid> ids, bool isPermanent = false);
        Task<ApiResult<BatchOperationResultDTO>> RestoreSchedulesAsync(List<Guid> ids);

        // Soft Delete Operations
        Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSoftDeletedSchedulesAsync(
            int pageNumber, int pageSize, string? searchTerm = null);

        // Business Operations
        Task<ApiResult<List<VaccinationScheduleResponseDTO>>> GetPendingSchedulesAsync();
        Task<ApiResult<List<VaccinationScheduleResponseDTO>>> GetInProgressSchedulesAsync();
        Task<ApiResult<bool>> StartScheduleAsync(Guid scheduleId);
        Task<ApiResult<bool>> CompleteScheduleAsync(Guid scheduleId);
    }
}