namespace Repositories.Interfaces
{
    public interface IMedicationLotRepository : IGenericRepository<MedicationLot, Guid>
    {
        #region Business Logic Methods
        Task<PagedList<MedicationLot>> GetMedicationLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicationId = null, bool? isExpired = null);
        Task<List<MedicationLot>> GetExpiringLotsAsync(int daysBeforeExpiry = 30);
        Task<List<MedicationLot>> GetLotsByMedicationIdAsync(Guid medicationId);
        Task<bool> LotNumberExistsAsync(string lotNumber, Guid? excludeId = null);
        Task<int> GetAvailableQuantityAsync(Guid medicationId);
        Task<List<MedicationLot>> GetExpiredLotsAsync();
        Task<bool> UpdateQuantityAsync(Guid lotId, int newQuantity);
        Task<MedicationLot?> GetLotWithMedicationAsync(Guid lotId);
        #endregion

        #region Extended Soft Delete Methods
        Task<PagedList<MedicationLot>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<MedicationLot?> GetByIdAsync(Guid id, bool includeDeleted = false);
        Task<bool> SoftDeleteLotAsync(Guid id, Guid deletedBy);
        Task<bool> RestoreLotAsync(Guid id, Guid restoredBy);
        Task<int> PermanentDeleteExpiredLotsAsync(int daysToExpire = 30);
        #endregion

        #region Statistics Methods
        Task<int> GetActiveLotCountAsync();
        Task<int> GetExpiredLotCountAsync();
        Task<int> GetExpiringLotCountAsync(int daysBeforeExpiry);
        Task<int> GetTotalLotCountAsync();
        Task<MedicationLotStatisticsResponseDTO> GetAllStatisticsAsync(DateTime currentDate, DateTime expiryThreshold);
        #endregion

        #region Batch Operations
        Task<List<MedicationLot>> GetMedicationLotsByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<int> SoftDeleteLotsAsync(List<Guid> ids, Guid deletedBy);
        Task<int> RestoreLotsAsync(List<Guid> ids, Guid restoredBy);
        Task<int> PermanentDeleteLotsAsync(List<Guid> ids);
        #endregion
    }
}