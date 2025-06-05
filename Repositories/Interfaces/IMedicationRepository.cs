namespace Repositories.Interfaces
{
    public interface IMedicationRepository : IGenericRepository<Medication, Guid>
    {
        Task<PagedList<Medication>> GetMedicationsAsync(int pageNumber, int pageSize, string? searchTerm = null, MedicationCategory? category = null);
        Task<bool> MedicationNameExistsAsync(string name, Guid? excludeId = null);
        Task<List<Medication>> GetMedicationsByCategoryAsync(MedicationCategory category);
        Task<List<Medication>> GetActiveMedicationsAsync();
        Task<int> GetTotalQuantityByMedicationIdAsync(Guid medicationId);
    }
}