using DTOs.VaccineLotDTOs.Response;

namespace Repositories.Interfaces
{
    public interface IVaccineLotRepository : IGenericRepository<MedicationLot, Guid>
    {
        #region Basic CRUD Methods
        Task<PagedList<MedicationLot>> GetVaccineLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? vaccineTypeId = null, bool? isExpired = null);
        Task<MedicationLot?> GetVaccineLotWithDetailsAsync(Guid lotId);
        Task<MedicationLot?> GetVaccineLotByIdAsync(Guid id, bool includeDeleted = false);
        #endregion

        #region Batch Operations    
        Task<int> SoftDeleteVaccineLotsAsync(List<Guid> ids, Guid deletedBy);
        Task<int> RestoreVaccineLotsAsync(List<Guid> ids, Guid restoredBy);
        #endregion

        #region Vaccine-Specific Operations
        Task<List<MedicationLot>> GetLotsByVaccineTypeAsync(Guid vaccineTypeId);
        Task<List<MedicationLot>> GetExpiringVaccineLotsAsync(int daysBeforeExpiry = 30);
        Task<List<MedicationLot>> GetExpiredVaccineLotsAsync();
        Task<bool> UpdateVaccineQuantityAsync(Guid lotId, int newQuantity);
        Task<bool> VaccineLotNumberExistsAsync(string lotNumber, Guid? excludeId = null);
        #endregion

        #region Soft Delete Operations
        Task<PagedList<MedicationLot>> GetSoftDeletedVaccineLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        #endregion

        #region Statistics Methods
        Task<VaccineLotStatisticsResponseDTO> GetVaccineLotStatisticsAsync(DateTime currentDate, DateTime expiryThreshold);
        #endregion
    }
}