using DTOs.ParentVaccinationDTOs.Request;
using DTOs.ParentVaccinationDTOs.Response;
using DTOs.VaccinationScheduleDTOs.Response;

namespace Services.Interfaces
{
    public interface IParentVaccinationService
    {
        // Lấy danh sách lịch tiêm theo trạng thái
        Task<ApiResult<PagedList<ParentVaccinationScheduleResponseDTO>>> GetVaccinationSchedulesByStatusAsync(
            ParentActionStatus status, int pageNumber = 1, int pageSize = 10);

        // Lấy chi tiết lịch tiêm
        Task<ApiResult<ParentVaccinationScheduleResponseDTO>> GetVaccinationScheduleDetailAsync(Guid scheduleId);

        // Ký đồng ý/từ chối tiêm chủng
        Task<ApiResult<bool>> SubmitConsentAsync(ParentConsentRequestDTO request);
        Task<ApiResult<bool>> SubmitBatchConsentAsync(BatchParentConsentRequestDTO request);

        // Lịch sử tiêm chủng
        Task<ApiResult<List<ParentVaccinationHistoryResponseDTO>>> GetVaccinationHistoryAsync();
        Task<ApiResult<ParentVaccinationHistoryResponseDTO>> GetStudentVaccinationHistoryAsync(Guid studentId);

        // Báo cáo phản ứng sau tiêm
        //Task<ApiResult<bool>> ReportVaccinationReactionAsync(ReportVaccinationReactionRequestDTO request);

        // Lấy thông báo mới
        Task<ApiResult<List<ParentVaccinationScheduleResponseDTO>>> GetPendingNotificationsAsync();

        // Thống kê tổng quan
        Task<ApiResult<ParentVaccinationSummaryDTO>> GetVaccinationSummaryAsync();

        Task<ApiResult<List<ParentVaccinationCreateResultDTO>>> CreateParentVaccinationListAsync(List<CreateParentVaccinationRequestDTO> requests);

        Task<ApiResult<List<ParentVaccinationCreateResultDTO>>> UpdateParentVaccinationListAsync(List<UpdateParentVaccinationRequestDTO> requests);

        Task<ApiResult<List<ParentVaccinationCreateResultDTO>>> SoftDeleteParentVaccinationRangeAsync(List<Guid> ids);
        Task<ApiResult<List<ParentVaccinationCreateResultDTO>>> RestoreParentVaccinationRangeAsync(List<Guid> ids);
        Task<ApiResult<List<ParentVaccinationRespondDTO>>> GetByStudentIdAsync(Guid studentId);
        Task<ApiResult<List<ParentVaccinationRespondDTO>>> GetByStudentCodeAsync(string studentCode);
        Task<ApiResult<List<ParentVaccinationRespondDTO>>> GetByParentUserIdAsync(Guid parentUserId);


    }
}