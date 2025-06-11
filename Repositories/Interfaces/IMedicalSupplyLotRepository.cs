namespace Repositories.Interfaces
{
    public interface IMedicalSupplyLotRepository : IGenericRepository<MedicalSupplyLot, Guid>
    {
        #region Query Operations
        Task<PagedList<MedicalSupplyLot>> GetMedicalSupplyLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicalSupplyId = null, bool? isExpired = null);
        Task<MedicalSupplyLot?> GetLotWithSupplyAsync(Guid id);
        Task<List<MedicalSupplyLot>> GetMedicalSupplyLotsByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<PagedList<MedicalSupplyLot>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        #endregion

        #region Business Logic Operations
        Task<List<MedicalSupplyLot>> GetExpiringLotsAsync(int daysBeforeExpiry = 30);
        Task<List<MedicalSupplyLot>> GetExpiredLotsAsync();
        Task<List<MedicalSupplyLot>> GetLotsByMedicalSupplyIdAsync(Guid medicalSupplyId);
        Task<int> GetAvailableQuantityAsync(Guid medicalSupplyId);
        Task<bool> UpdateQuantityAsync(Guid lotId, int newQuantity);
        Task<int> CalculateCurrentStockForSupplyAsync(Guid medicalSupplyId);
        #endregion

        #region Validation Operations
        Task<bool> LotNumberExistsAsync(string lotNumber, Guid? excludeId = null);
        #endregion

        #region Unified Delete & Restore Operations
        Task<int> SoftDeleteLotsAsync(List<Guid> ids, Guid deletedBy);
        Task<int> RestoreLotsAsync(List<Guid> ids, Guid restoredBy);
        Task<int> PermanentDeleteLotsAsync(List<Guid> ids);
        Task<int> PermanentDeleteExpiredLotsAsync(int daysExpired = 90);
        #endregion

        #region Additional Helper Methods
        Task<bool> HasActiveLotsAsync(Guid medicalSupplyId);
        #endregion
    }
}