namespace Repositories.Interfaces
{
    public interface IVaccineDoseInfoRepository : IGenericRepository<VaccineDoseInfo, Guid>
    {
        Task<PagedList<VaccineDoseInfo>> GetVaccineDoseInfosAsync(
            int pageNumber, int pageSize, Guid? vaccineTypeId = null, int? doseNumber = null);

        Task<VaccineDoseInfo?> GetVaccineDoseInfoByIdAsync(Guid id);
        Task<VaccineDoseInfo?> GetVaccineDoseInfoWithDetailsAsync(Guid id);
        Task<List<VaccineDoseInfo>> GetVaccineDoseInfosByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<List<VaccineDoseInfo>> GetDoseInfosByVaccineTypeAsync(Guid vaccineTypeId);
        Task<VaccineDoseInfo?> GetDoseInfoByVaccineTypeAndDoseNumberAsync(Guid vaccineTypeId, int doseNumber);
        Task<bool> IsDoseNumberExistsAsync(Guid vaccineTypeId, int doseNumber, Guid? excludeId = null);
        Task<List<VaccineDoseInfo>> GetNextDosesAsync(Guid currentDoseId);
        Task<int> GetMaxDoseNumberByVaccineTypeAsync(Guid vaccineTypeId);
    }
}