namespace Repositories.Interfaces
{
    public interface IMedicalSupplyRepository : IGenericRepository<MedicalSupply, Guid>
    {
        #region Query Operations
        Task<PagedList<MedicalSupply>> GetMedicalSuppliesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<MedicalSupply?> GetSupplyWithLotsAsync(Guid id);
        Task<List<MedicalSupply>> GetMedicalSuppliesByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<PagedList<MedicalSupply>> GetSoftDeletedSuppliesAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        #endregion

        #region Business Logic Operations
        Task<List<MedicalSupply>> GetLowStockSuppliesAsync();
        Task<bool> UpdateCurrentStockAsync(Guid id, int newStock);
        Task<bool> UpdateMinimumStockAsync(Guid id, int newMinimumStock);
        #endregion

        #region Validation Operations
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
        #endregion

        #region Unified Batch Operations
        Task<int> SoftDeleteSuppliesAsync(List<Guid> ids, Guid deletedBy);
        Task<int> RestoreSuppliesAsync(List<Guid> ids, Guid restoredBy);
        Task<int> PermanentDeleteSuppliesAsync(List<Guid> ids);
        #endregion
    }
}