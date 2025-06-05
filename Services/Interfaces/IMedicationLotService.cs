using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;

namespace Services.Interfaces
{
    public interface IMedicationLotService
    {
        Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetMedicationLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicationId = null, bool? isExpired = null);
        Task<ApiResult<MedicationLotResponseDTO>> GetMedicationLotByIdAsync(Guid id);
        Task<ApiResult<MedicationLotResponseDTO>> CreateMedicationLotAsync(CreateMedicationLotRequest request);
        Task<ApiResult<MedicationLotResponseDTO>> UpdateMedicationLotAsync(Guid id, UpdateMedicationLotRequest request);
        Task<ApiResult<bool>> DeleteMedicationLotAsync(Guid id);
        Task<ApiResult<MedicationLotResponseDTO>> RestoreMedicationLotAsync(Guid id);
        Task<ApiResult<bool>> PermanentDeleteMedicationLotAsync(Guid id);
        Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiringLotsAsync(int daysBeforeExpiry = 30);
        Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiredLotsAsync();
        Task<ApiResult<List<MedicationLotResponseDTO>>> GetLotsByMedicationIdAsync(Guid medicationId);
        Task<ApiResult<int>> GetAvailableQuantityAsync(Guid medicationId);
        Task<ApiResult<bool>> UpdateQuantityAsync(Guid lotId, int newQuantity);
        Task<ApiResult<int>> CleanupExpiredLotsAsync(int daysToExpire = 90);
    }
}