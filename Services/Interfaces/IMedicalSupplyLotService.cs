namespace Services.Interfaces
{
    public interface IMedicalSupplyLotService
    {
        #region Basic CRUD Operations
        Task<ApiResult<PagedList<MedicalSupplyLotResponseDTO>>> GetMedicalSupplyLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicalSupplyId = null, bool? isExpired = null, bool includeDeleted = false);
        Task<ApiResult<MedicalSupplyLotResponseDTO>> GetMedicalSupplyLotByIdAsync(Guid id);
        Task<ApiResult<MedicalSupplyLotResponseDTO>> CreateMedicalSupplyLotAsync(CreateMedicalSupplyLotRequest request);
        Task<ApiResult<MedicalSupplyLotResponseDTO>> UpdateMedicalSupplyLotAsync(Guid id, UpdateMedicalSupplyLotRequest request);
        #endregion

        #region Unified Delete & Restore Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteMedicalSupplyLotsAsync(List<Guid> ids, bool isPermanent = false);
        Task<ApiResult<BatchOperationResultDTO>> RestoreMedicalSupplyLotsAsync(List<Guid> ids);
        #endregion

        #region Soft Delete Operations
        Task<ApiResult<PagedList<MedicalSupplyLotResponseDTO>>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        #endregion

        #region Business Logic Operations
        Task<ApiResult<List<MedicalSupplyLotResponseDTO>>> GetExpiringLotsAsync(int daysBeforeExpiry = 30);
        Task<ApiResult<List<MedicalSupplyLotResponseDTO>>> GetExpiredLotsAsync();
        Task<ApiResult<List<MedicalSupplyLotResponseDTO>>> GetLotsByMedicalSupplyIdAsync(Guid medicalSupplyId);
        Task<ApiResult<int>> GetAvailableQuantityAsync(Guid medicalSupplyId);
        Task<ApiResult<bool>> UpdateQuantityAsync(Guid lotId, int newQuantity);
        #endregion
    }
}