namespace Services.Interfaces
{
    public interface IVaccineLotService
    {
        #region Basic CRUD Operations
        Task<ApiResult<PagedList<VaccineLotResponseDTO>>> GetVaccineLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? vaccineTypeId = null, bool? isExpired = null);
        Task<ApiResult<VaccineLotResponseDTO>> GetVaccineLotByIdAsync(Guid id);
        Task<ApiResult<VaccineLotResponseDTO>> CreateVaccineLotAsync(CreateVaccineLotRequest request);
        Task<ApiResult<VaccineLotResponseDTO>> UpdateVaccineLotAsync(Guid id, UpdateVaccineLotRequest request);
        #endregion

        #region Batch Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineLotsAsync(List<Guid> ids);
        Task<ApiResult<BatchOperationResultDTO>> RestoreVaccineLotsAsync(List<Guid> ids);
        #endregion

        #region Business Logic Operations
        Task<ApiResult<List<VaccineLotResponseDTO>>> GetExpiringVaccineLotsAsync(int daysBeforeExpiry = 30);
        Task<ApiResult<List<VaccineLotResponseDTO>>> GetExpiredVaccineLotsAsync();
        Task<ApiResult<List<VaccineLotResponseDTO>>> GetLotsByVaccineTypeAsync(Guid vaccineTypeId);
        Task<ApiResult<int>> GetAvailableVaccineQuantityAsync(Guid vaccineTypeId);
        Task<ApiResult<bool>> UpdateVaccineQuantityAsync(Guid lotId, int newQuantity);
        #endregion

        #region Soft Delete Operations
        Task<ApiResult<PagedList<VaccineLotResponseDTO>>> GetSoftDeletedVaccineLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        #endregion

        #region Statistics
        Task<ApiResult<VaccineLotStatisticsResponseDTO>> GetVaccineLotStatisticsAsync();
        #endregion
    }
}