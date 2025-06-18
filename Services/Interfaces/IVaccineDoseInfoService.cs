namespace Services.Interfaces
{
    public interface IVaccineDoseInfoService
    {
        // Basic CRUD Operations
        Task<ApiResult<PagedList<VaccineDoseInfoResponseDTO>>> GetVaccineDoseInfosAsync(
            int pageNumber, int pageSize, Guid? vaccineTypeId = null, int? doseNumber = null);

        Task<ApiResult<VaccineDoseInfoResponseDTO>> GetVaccineDoseInfoByIdAsync(Guid id);
        Task<ApiResult<VaccineDoseInfoDetailResponseDTO>> GetVaccineDoseInfoDetailByIdAsync(Guid id);
        Task<ApiResult<VaccineDoseInfoResponseDTO>> CreateVaccineDoseInfoAsync(CreateVaccineDoseInfoRequest request);
        Task<ApiResult<VaccineDoseInfoResponseDTO>> UpdateVaccineDoseInfoAsync(Guid id, UpdateVaccineDoseInfoRequest request);

        // Batch Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineDoseInfosAsync(List<Guid> ids, bool isPermanent = false);

        // Business Operations
        Task<ApiResult<List<VaccineDoseInfoResponseDTO>>> GetDoseInfosByVaccineTypeAsync(Guid vaccineTypeId);
        Task<ApiResult<VaccineDoseInfoResponseDTO>> GetNextRecommendedDoseAsync(Guid vaccineTypeId, int currentDoseNumber);
    }
}