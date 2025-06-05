using DTOs.MedicationDTOs.Request;
using DTOs.MedicationDTOs.Response;

namespace Services.Interfaces
{
    public interface IMedicationService
    {
        /// <summary>
        /// Lấy danh sách thuốc theo phân trang, có thể tìm kiếm và lọc theo danh mục.
        /// </summary>
        Task<ApiResult<PagedList<MedicationResponse>>> GetMedicationsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            MedicationCategory? category = null);

        /// <summary>
        /// Lấy thông tin chi tiết của một thuốc theo ID.
        /// </summary>
        Task<ApiResult<MedicationResponse>> GetMedicationByIdAsync(Guid id);

        /// <summary>
        /// Tạo mới một thuốc.
        /// </summary>
        Task<ApiResult<MedicationResponse>> CreateMedicationAsync(CreateMedicationRequest request);

        /// <summary>
        /// Cập nhật thông tin thuốc theo ID.
        /// </summary>
        Task<ApiResult<MedicationResponse>> UpdateMedicationAsync(Guid id, UpdateMedicationRequest request);

        /// <summary>
        /// Xóa mềm (soft delete) một thuốc theo ID.
        /// </summary>
        Task<ApiResult<string>> DeleteMedicationAsync(Guid id);

        /// <summary>
        /// Lấy danh sách thuốc theo một danh mục cố định.
        /// </summary>
        Task<ApiResult<List<MedicationResponse>>> GetMedicationsByCategoryAsync(MedicationCategory category);

        /// <summary>
        /// Lấy danh sách các thuốc đang ở trạng thái Active.
        /// </summary>
        Task<ApiResult<List<MedicationResponse>>> GetActiveMedicationsAsync();
    }
}
