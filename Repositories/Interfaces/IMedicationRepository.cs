using DTOs.MedicationDTOs.Response;

namespace Repositories.Interfaces
{
    public interface IMedicationRepository : IGenericRepository<Medication, Guid>
    {
        // Specific medication business logic methods
        Task<PagedList<Medication>> GetMedicationsAsync(int pageNumber, int pageSize, string? searchTerm = null, MedicationCategory? category = null);
        Task<bool> MedicationNameExistsAsync(string name, Guid? excludeId = null);
        Task<List<Medication>> GetMedicationsByCategoryAsync(MedicationCategory category);
        Task<List<Medication>> GetActiveMedicationsAsync();
        Task<int> GetTotalQuantityByMedicationIdAsync(Guid medicationId);

        // Extended soft delete methods với business logic
        Task<bool> SoftDeleteWithLotsAsync(Guid id, Guid deletedBy);
        Task<bool> RestoreWithLotsAsync(Guid id, Guid restoredBy);
        Task<bool> PermanentDeleteWithLotsAsync(Guid id);
        Task<PagedList<Medication>> GetSoftDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<int> PermanentDeleteExpiredAsync(int daysToExpire = 30);
    }
}