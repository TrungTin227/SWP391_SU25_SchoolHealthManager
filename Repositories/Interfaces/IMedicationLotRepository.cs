namespace Repositories.Interfaces
{
    public interface IMedicationLotRepository : IGenericRepository<MedicationLot, Guid>
    {
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
        Task<PagedList<MedicationLot>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        // Thêm method với includeDeleted parameter
        Task<MedicationLot?> GetByIdAsync(Guid id, bool includeDeleted = false);

        // Thêm các method cho soft delete operations
        new Task<MedicationLot> AddAsync(MedicationLot entity);
        new Task UpdateAsync(MedicationLot entity);
        new Task SoftDeleteAsync(MedicationLot entity);
        new Task DeleteAsync(MedicationLot entity);
    }
}