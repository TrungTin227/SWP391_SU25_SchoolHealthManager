namespace Repositories.Interfaces
{
    public interface IVaccineTypeRepository : IGenericRepository<VaccinationType, Guid>
    {
        Task<PagedList<VaccinationType>> GetVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);

        Task<VaccinationType?> GetVaccineTypeByIdAsync(Guid id);
        Task<VaccinationType?> GetVaccineTypeWithDetailsAsync(Guid id);
        Task<VaccinationType?> GetByCodeAsync(string code);
        Task<List<VaccinationType>> GetVaccineTypesByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<PagedList<VaccinationType>> GetSoftDeletedVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<List<VaccinationType>> GetActiveVaccineTypesAsync();
    }
}