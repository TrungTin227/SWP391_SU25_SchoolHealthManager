namespace Services.Interfaces
{
    public interface IMedicationLotService
    {
        Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetMedicationLotsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            Guid? medicationId = null,
            bool? isExpired = null,
            int? daysBeforeExpiry = null,
            bool includeDeleted = false);

        Task<ApiResult<MedicationLotResponseDTO>> GetMedicationLotByIdAsync(Guid id);
        Task<ApiResult<MedicationLotResponseDTO>> CreateMedicationLotAsync(CreateMedicationLotRequest request);
        Task<ApiResult<MedicationLotResponseDTO>> UpdateMedicationLotAsync(Guid id, UpdateMedicationLotRequest request);
        Task<ApiResult<bool>> UpdateQuantityAsync(Guid id, int newQuantity);

        Task<ApiResult<BatchOperationResultDTO>> DeleteMedicationLotsAsync(List<Guid> ids, bool isPermanent = false);
        Task<ApiResult<BatchOperationResultDTO>> RestoreMedicationLotsAsync(List<Guid> ids);

        #region Statistics
        Task<ApiResult<MedicationLotStatisticsResponseDTO>> GetStatisticsAsync();
        #endregion
    }
}