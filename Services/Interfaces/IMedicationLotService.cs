﻿using DTOs.Common;
using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;

namespace Services.Interfaces
{
    public interface IMedicationLotService
    {
        #region Basic CRUD Operations
        Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetMedicationLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicationId = null, bool? isExpired = null);
        Task<ApiResult<MedicationLotResponseDTO>> GetMedicationLotByIdAsync(Guid id);
        Task<ApiResult<MedicationLotResponseDTO>> CreateMedicationLotAsync(CreateMedicationLotRequest request);
        Task<ApiResult<MedicationLotResponseDTO>> UpdateMedicationLotAsync(Guid id, UpdateMedicationLotRequest request);
        Task<ApiResult<bool>> DeleteMedicationLotAsync(Guid id);
        #endregion

        #region Soft Delete Operations
        Task<ApiResult<MedicationLotResponseDTO>> RestoreMedicationLotAsync(Guid id);
        Task<ApiResult<bool>> PermanentDeleteMedicationLotAsync(Guid id);
        Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<ApiResult<BatchOperationResultDTO>> CleanupExpiredLotsAsync(int daysToExpire = 90);
        #endregion

        #region Business Logic Operations
        Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiringLotsAsync(int daysBeforeExpiry = 30);
        Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiredLotsAsync();
        Task<ApiResult<List<MedicationLotResponseDTO>>> GetLotsByMedicationIdAsync(Guid medicationId);
        Task<ApiResult<int>> GetAvailableQuantityAsync(Guid medicationId);
        Task<ApiResult<bool>> UpdateQuantityAsync(Guid lotId, int newQuantity);
        #endregion

        #region Statistics
        Task<ApiResult<MedicationLotStatisticsResponseDTO>> GetStatisticsAsync();
        #endregion

        #region Batch Delete Operations
        Task<ApiResult<BatchOperationResultDTO>> DeleteMedicationLotsAsync(List<Guid> ids);
        Task<ApiResult<BatchOperationResultDTO>> RestoreMedicationLotsAsync(List<Guid> ids);
        Task<ApiResult<BatchOperationResultDTO>> PermanentDeleteMedicationLotsAsync(List<Guid> ids);
        #endregion
    }
}