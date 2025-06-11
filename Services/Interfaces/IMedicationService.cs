using DTOs.MedicationDTOs.Request;
using DTOs.MedicationDTOs.Response;

namespace Services.Interfaces
{
    public interface IMedicationService
    {
        /// <summary>
        /// Lấy danh sách thuốc theo phân trang, có thể tìm kiếm, lọc theo danh mục và bao gồm thuốc đã xóa.
        /// </summary>
        Task<ApiResult<PagedList<MedicationResponse>>> GetMedicationsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            MedicationCategory? category = null,
            bool includeDeleted = false);

        /// <summary>
        /// Lấy thông tin chi tiết của một thuốc theo ID.
        /// </summary>
        Task<ApiResult<MedicationResponse>> GetMedicationByIdAsync(Guid id);

        /// <summary>
        /// Lấy thông tin chi tiết thuốc kèm thông tin lô.
        /// </summary>
        Task<ApiResult<MedicationDetailResponse>> GetMedicationDetailByIdAsync(Guid id);

        /// <summary>
        /// Tạo mới một thuốc.
        /// </summary>
        Task<ApiResult<MedicationResponse>> CreateMedicationAsync(CreateMedicationRequest request);

        /// <summary>
        /// Cập nhật thông tin thuốc theo ID.
        /// </summary>
        Task<ApiResult<MedicationResponse>> UpdateMedicationAsync(Guid id, UpdateMedicationRequest request);

        /// <summary>
        /// Xóa thuốc (hỗ trợ xóa 1 hoặc nhiều, soft delete hoặc permanent).
        /// </summary>
        Task<ApiResult<BatchOperationResultDTO>> DeleteMedicationsAsync(List<Guid> medicationIds, bool isPermanent = false);

        /// <summary>
        /// Khôi phục thuốc đã bị soft delete (hỗ trợ 1 hoặc nhiều).
        /// </summary>
        Task<ApiResult<BatchOperationResultDTO>> RestoreMedicationsAsync(List<Guid> medicationIds);

        /// <summary>
        /// Xóa vĩnh viễn các thuốc đã soft delete quá thời hạn.
        /// </summary>
        Task<ApiResult<string>> CleanupExpiredMedicationsAsync(int daysToExpire = 30);

        /// <summary>
        /// Lấy danh sách các thuốc đang ở trạng thái Active.
        /// </summary>
        Task<ApiResult<List<MedicationResponse>>> GetActiveMedicationsAsync();
    }
}