namespace Repositories.Interfaces
{
    public interface IMedicationLotRepository : IGenericRepository<MedicationLot, Guid>
    {
        #region Basic CRUD Methods
        Task<PagedList<MedicationLot>> GetMedicationLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicationId = null, bool? isExpired = null);
        Task<MedicationLot?> GetLotWithMedicationAsync(Guid lotId);
        Task<MedicationLot?> GetByIdAsync(Guid id, bool includeDeleted = false);
        #endregion

        #region Batch Operations (Unified - Support Single and Multiple)
        Task<List<MedicationLot>> GetMedicationLotsByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<int> SoftDeleteLotsAsync(List<Guid> ids, Guid deletedBy);
        Task<int> RestoreLotsAsync(List<Guid> ids, Guid restoredBy);
        Task<int> PermanentDeleteLotsAsync(List<Guid> ids);
        #endregion

        #region Soft Delete Operations
        Task<PagedList<MedicationLot>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<int> PermanentDeleteExpiredLotsAsync(int daysToExpire = 30);
        #endregion

        #region Business Logic Methods
        Task<List<MedicationLot>> GetExpiringLotsAsync(int daysBeforeExpiry = 30);
        Task<List<MedicationLot>> GetExpiredLotsAsync();
        Task<List<MedicationLot>> GetLotsByMedicationIdAsync(Guid medicationId);
        Task<int> GetAvailableQuantityAsync(Guid medicationId);
        Task<bool> UpdateQuantityAsync(Guid lotId, int newQuantity);
        Task<bool> LotNumberExistsAsync(string lotNumber, Guid? excludeId = null);
        #endregion

        #region Statistics Methods
        Task<MedicationLotStatisticsResponseDTO> GetAllStatisticsAsync(DateTime currentDate, DateTime expiryThreshold);
        #endregion
    }
}