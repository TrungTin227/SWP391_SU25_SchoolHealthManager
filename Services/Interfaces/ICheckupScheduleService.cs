namespace Services.Interfaces
{
    public interface ICheckupScheduleService
    {
        // Basic CRUD Operations
        Task<ApiResult<PagedList<CheckupScheduleResponseDTO>>> GetCheckupSchedulesAsync(
            int pageNumber, int pageSize, Guid? campaignId = null,
            CheckupScheduleStatus? status = null, string? searchTerm = null);

        Task<ApiResult<CheckupScheduleDetailResponseDTO>> GetCheckupScheduleByIdAsync(Guid id);
        Task<ApiResult<List<CheckupScheduleDetailResponseDTO>>> GetCheckupScheduleByStudentIdAsync(Guid id);
        Task<ApiResult<List<CheckupScheduleResponseDTO>>> CreateCheckupSchedulesAsync(CreateCheckupScheduleRequest request);
        Task<ApiResult<CheckupScheduleResponseDTO>> UpdateCheckupScheduleAsync(UpdateCheckupScheduleRequest request);

        // Consent Management
        Task<ApiResult<CheckupScheduleResponseDTO>> UpdateConsentStatusAsync(UpdateConsentStatusRequest request);

        Task<ApiResult<BatchOperationResultDTO>> BatchUpdateScheduleStatusAsync(CheckupBatchUpdateStatusRequest request);
        Task<ApiResult<BatchOperationResultDTO>> BatchDeleteSchedulesAsync(CheckupBatchDeleteScheduleRequest request);
        Task<ApiResult<BatchOperationResultDTO>> BatchRestoreSchedulesAsync(CheckupBatchRestoreScheduleRequest request);

        // Campaign Related
        Task<ApiResult<List<CheckupScheduleResponseDTO>>> GetSchedulesByCampaignAsync(Guid campaignId);

        // Statistics
        Task<ApiResult<Dictionary<CheckupScheduleStatus, int>>> GetScheduleStatusStatisticsAsync(Guid? campaignId = null);
        Task<ApiResult<List<CheckupScheduleForParentResponseDTO>>> GetSchedulesForParentAsync();
    }
}