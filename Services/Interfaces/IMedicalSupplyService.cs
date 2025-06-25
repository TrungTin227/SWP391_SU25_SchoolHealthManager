namespace Services.Interfaces
{
    public interface IMedicalSupplyService
    {
        #region Basic CRUD Operations
        Task<ApiResult<PagedList<MedicalSupplyResponseDTO>>> GetMedicalSuppliesAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null,
            bool includeDeleted = false);
        Task<ApiResult<MedicalSupplyResponseDTO>> GetMedicalSupplyByIdAsync(Guid id);
        Task<ApiResult<MedicalSupplyDetailResponseDTO>> GetMedicalSupplyDetailByIdAsync(Guid id);
        Task<ApiResult<MedicalSupplyResponseDTO>> CreateMedicalSupplyAsync(CreateMedicalSupplyRequest request);
        Task<ApiResult<MedicalSupplyResponseDTO>> UpdateMedicalSupplyAsync(Guid id, UpdateMedicalSupplyRequest request);
        #endregion

        #region Soft Delete Operations
        Task<ApiResult<PagedList<MedicalSupplyResponseDTO>>> GetSoftDeletedSuppliesAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null);
        #endregion

        #region Unified Delete & Restore Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteMedicalSuppliesAsync(List<Guid> ids, bool isPermanent = false);
        Task<ApiResult<BatchOperationResultDTO>> RestoreMedicalSuppliesAsync(List<Guid> ids);
        #endregion

        #region Business Logic Operations
        Task<ApiResult<List<MedicalSupplyResponseDTO>>> GetLowStockSuppliesAsync();
        Task<ApiResult<bool>> UpdateCurrentStockAsync(Guid id, int newStock);
        Task<ApiResult<bool>> UpdateMinimumStockAsync(Guid id, int newMinimumStock);
        #endregion
    }
}