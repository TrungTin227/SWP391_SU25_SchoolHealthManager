namespace Services.Interfaces
{
    public interface IVaccineTypeService
    {
        // Basic CRUD Operations
        Task<ApiResult<PagedList<VaccineTypeResponseDTO>>> GetVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);

        Task<ApiResult<VaccineTypeResponseDTO>> GetVaccineTypeByIdAsync(Guid id);
        Task<ApiResult<VaccineTypeDetailResponseDTO>> GetVaccineTypeDetailByIdAsync(Guid id);
        Task<ApiResult<VaccineTypeResponseDTO>> CreateVaccineTypeAsync(CreateVaccineTypeRequest request);
        Task<ApiResult<VaccineTypeResponseDTO>> UpdateVaccineTypeAsync(Guid id, UpdateVaccineTypeRequest request);

        // Batch Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineTypesAsync(List<Guid> ids, bool isPermanent = false);
        Task<ApiResult<BatchOperationResultDTO>> RestoreVaccineTypesAsync(List<Guid> ids);

        // Soft Delete Operations
        Task<ApiResult<PagedList<VaccineTypeResponseDTO>>> GetSoftDeletedVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null);

        // Business Operations
        Task<ApiResult<List<VaccineTypeResponseDTO>>> GetActiveVaccineTypesAsync();
        Task<ApiResult<VaccineTypeResponseDTO>> ToggleVaccineTypeStatusAsync(Guid id);
    }
}