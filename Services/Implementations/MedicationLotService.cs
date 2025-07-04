﻿using Microsoft.Extensions.Logging;
using Services.Mappers;
using System.Data;

namespace Services.Implementations
{
    public class MedicationLotService : BaseService<MedicationLot, Guid>, IMedicationLotService
    {
        private readonly ILogger<MedicationLotService> _logger;

        public MedicationLotService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<MedicationLotService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.MedicationLotRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetMedicationLotsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            Guid? medicationId = null,
            bool? isExpired = null,
            int? daysBeforeExpiry = null,
            bool includeDeleted = false)
        {
            try
            {
                var lots = await _unitOfWork.MedicationLotRepository.GetMedicationLotsAsync(
                    pageNumber, pageSize, searchTerm, medicationId, isExpired, daysBeforeExpiry, includeDeleted);

                var result = MedicationLotMapper.MapToPagedResponseDTO(lots);

                return ApiResult<PagedList<MedicationLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication lots");
                return ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicationLotResponseDTO>> GetMedicationLotByIdAsync(Guid id)
        {
            try
            {
                var lot = await _unitOfWork.MedicationLotRepository.GetLotWithMedicationAsync(id);
                if (lot == null)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không tìm thấy lô thuốc"));
                }

                var lotDTO = MedicationLotMapper.MapToResponseDTO(lot);
                return ApiResult<MedicationLotResponseDTO>.Success(
                    lotDTO, "Lấy thông tin lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication lot by ID: {LotId}", id);
                return ApiResult<MedicationLotResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicationLotResponseDTO>> CreateMedicationLotAsync(CreateMedicationLotRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = await ValidateCreateRequestAsync(request);
                    if (!validationResult.IsSuccess)
                    {
                        return ApiResult<MedicationLotResponseDTO>.Failure(
                            new Exception(validationResult.Message));
                    }

                    var lot = MedicationLotMapper.MapFromCreateRequest(request);

                    // Use BaseService's CreateAsync method - it handles audit fields and SaveChanges
                    var createdLot = await CreateAsync(lot);

                    var lotWithMedication = await _unitOfWork.MedicationLotRepository.GetLotWithMedicationAsync(createdLot.Id);
                    if (lotWithMedication == null)
                    {
                        return ApiResult<MedicationLotResponseDTO>.Failure(
                            new Exception("Không thể lấy thông tin lô thuốc vừa tạo"));
                    }

                    var lotDTO = MedicationLotMapper.MapToResponseDTO(lotWithMedication);
                    return ApiResult<MedicationLotResponseDTO>.Success(
                        lotDTO, "Tạo lô thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating medication lot");
                    return ApiResult<MedicationLotResponseDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<MedicationLotResponseDTO>> UpdateMedicationLotAsync(
            Guid id, UpdateMedicationLotRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var lot = await _unitOfWork.MedicationLotRepository.GetByIdAsync(id);
                    if (lot == null)
                    {
                        return ApiResult<MedicationLotResponseDTO>.Failure(
                            new Exception("Không tìm thấy lô thuốc"));
                    }

                    var validationResult = await ValidateUpdateRequestAsync(request, id);
                    if (!validationResult.IsSuccess)
                    {
                        return ApiResult<MedicationLotResponseDTO>.Failure(
                            new Exception(validationResult.Message));
                    }

                    MedicationLotMapper.MapFromUpdateRequest(request, lot);

                    // Use BaseService's UpdateAsync method - it handles audit fields and SaveChanges
                    await UpdateAsync(lot);

                    var updatedLot = await _unitOfWork.MedicationLotRepository.GetLotWithMedicationAsync(id);
                    if (updatedLot == null)
                    {
                        return ApiResult<MedicationLotResponseDTO>.Failure(
                            new Exception("Không thể lấy thông tin lô thuốc đã cập nhật"));
                    }

                    var lotDTO = MedicationLotMapper.MapToResponseDTO(updatedLot);
                    return ApiResult<MedicationLotResponseDTO>.Success(
                        lotDTO, "Cập nhật lô thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating medication lot: {LotId}", id);
                    return ApiResult<MedicationLotResponseDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<bool>> DeleteMedicationLotAsync(Guid id)
        {
            try
            {
                // Use BaseService's DeleteAsync method - it handles soft delete and SaveChanges
                var success = await DeleteAsync(id);

                if (!success)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Không tìm thấy lô thuốc"));
                }

                return ApiResult<bool>.Success(true, "Xóa lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medication lot: {LotId}", id);
                return ApiResult<bool>.Failure(ex);
            }
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteMedicationLotsAsync(List<Guid> ids, bool isPermanent = false)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (ids == null || !ids.Any())
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException("Danh sách ID không được rỗng"));
                    }

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    if (isPermanent)
                    {
                        result = await ProcessPermanentDeleteBatch(ids, result);
                    }
                    else
                    {
                        result = await ProcessSoftDeleteBatch(ids, result);
                    }

                    var operationText = isPermanent ? "xóa vĩnh viễn" : "xóa";
                    result.Message = GenerateBatchOperationMessage(operationText, result);

                    _logger.LogInformation(
                        "Batch {OperationType} completed: {SuccessCount} success, {FailureCount} failures",
                        isPermanent ? "permanent delete" : "soft delete", result.SuccessCount, result.FailureCount);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    var operationType = isPermanent ? "permanent delete" : "soft delete";
                    _logger.LogError(ex, "Error during batch {OperationType} of medication lots", operationType);
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreMedicationLotsAsync(List<Guid> ids)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (ids == null || !ids.Any())
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException("Danh sách ID không được rỗng"));
                    }

                    _logger.LogInformation("Starting batch restore for {Count} medication lots", ids.Count);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    // Get deleted lots
                    var deletedLots = await _unitOfWork.MedicationLotRepository.GetMedicationLotsByIdsAsync(ids, includeDeleted: true);
                    var deletedLotIds = deletedLots.Where(l => l.IsDeleted).Select(l => l.Id).ToList();
                    var notDeletedIds = ids.Except(deletedLotIds).ToList();

                    // Add errors for non-deleted IDs
                    AddRestoreErrors(result, notDeletedIds, deletedLots);

                    // Restore deleted lots
                    if (deletedLotIds.Any())
                    {
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var restoredCount = await _unitOfWork.MedicationLotRepository.RestoreLotsAsync(deletedLotIds, currentUserId);

                        if (restoredCount > 0)
                        {
                            await _unitOfWork.SaveChangesAsync();
                            result.SuccessCount = restoredCount;
                            result.SuccessIds = deletedLotIds.Select(id => id.ToString()).ToList();
                        }
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

                    _logger.LogInformation(
                        "Batch restore completed: {SuccessCount} success, {FailureCount} failures",
                        result.SuccessCount, result.FailureCount);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch restore of medication lots");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Business Logic Operations

        public async Task<ApiResult<bool>> UpdateQuantityAsync(Guid lotId, int newQuantity)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (newQuantity < 0)
                    {
                        return ApiResult<bool>.Failure(
                            new Exception("Số lượng không được âm"));
                    }

                    var success = await _unitOfWork.MedicationLotRepository.UpdateQuantityAsync(lotId, newQuantity);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new Exception("Không tìm thấy lô thuốc"));
                    }

                    await _unitOfWork.SaveChangesAsync();
                    return ApiResult<bool>.Success(true, "Cập nhật số lượng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating quantity for lot: {LotId}", lotId);
                    return ApiResult<bool>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Statistics

        public async Task<ApiResult<MedicationLotStatisticsResponseDTO>> GetStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to calculate medication lot statistics");

                var currentDate = DateTime.UtcNow.Date;
                var expiryThreshold = currentDate.AddDays(30);

                var statistics = await _unitOfWork.MedicationLotRepository.GetAllStatisticsAsync(currentDate, expiryThreshold);

                _logger.LogInformation(
                    "Successfully calculated medication lot statistics: Total={TotalLots}, Active={ActiveLots}, Expired={ExpiredLots}, Expiring={ExpiringInNext30Days}",
                    statistics.TotalLots, statistics.ActiveLots, statistics.ExpiredLots, statistics.ExpiringInNext30Days);

                return ApiResult<MedicationLotStatisticsResponseDTO>.Success(
                    statistics, "Lấy thống kê lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating medication lot statistics");
                return ApiResult<MedicationLotStatisticsResponseDTO>.Failure(ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<BatchOperationResultDTO> ProcessPermanentDeleteBatch(List<Guid> ids, BatchOperationResultDTO result)
        {
            // Get all lots (including deleted ones)
            var allLots = await _unitOfWork.MedicationLotRepository.GetMedicationLotsByIdsAsync(ids, includeDeleted: true);
            var existingIds = allLots.Select(l => l.Id).ToList();
            var notFoundIds = ids.Except(existingIds).ToList();

            // Add errors for not found IDs
            AddNotFoundErrors(result, notFoundIds, "Không tìm thấy lô thuốc");

            // Perform permanent delete for existing lots
            if (existingIds.Any())
            {
                var deletedCount = await _unitOfWork.MedicationLotRepository.PermanentDeleteLotsAsync(existingIds);

                if (deletedCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    result.SuccessCount = deletedCount;
                    result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                }
            }

            return result;
        }

        private async Task<BatchOperationResultDTO> ProcessSoftDeleteBatch(List<Guid> ids, BatchOperationResultDTO result)
        {
            // Get only non-deleted lots
            var existingLots = await _unitOfWork.MedicationLotRepository.GetMedicationLotsByIdsAsync(ids, includeDeleted: false);
            var existingIds = existingLots.Select(l => l.Id).ToList();
            var notFoundIds = ids.Except(existingIds).ToList();

            // Add errors for not found IDs
            AddNotFoundErrors(result, notFoundIds, "Không tìm thấy lô thuốc hoặc đã bị xóa");

            // Perform soft delete for existing lots
            if (existingIds.Any())
            {
                var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                var deletedCount = await _unitOfWork.MedicationLotRepository.SoftDeleteLotsAsync(existingIds, currentUserId);

                if (deletedCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    result.SuccessCount = deletedCount;
                    result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                }
            }

            return result;
        }

        private void AddNotFoundErrors(BatchOperationResultDTO result, List<Guid> notFoundIds, string errorMessage)
        {
            foreach (var notFoundId in notFoundIds)
            {
                result.Errors.Add(new BatchOperationErrorDTO
                {
                    Id = notFoundId.ToString(),
                    Error = errorMessage,
                    Details = $"Medication lot với ID {notFoundId} {errorMessage.ToLower()}"
                });
            }
        }

        private void AddRestoreErrors(BatchOperationResultDTO result, List<Guid> notDeletedIds, List<MedicationLot> allLots)
        {
            foreach (var notDeletedId in notDeletedIds)
            {
                var lot = allLots.FirstOrDefault(l => l.Id == notDeletedId);
                string errorMessage = lot == null
                    ? "Không tìm thấy lô thuốc"
                    : "Lô thuốc chưa bị xóa";

                result.Errors.Add(new BatchOperationErrorDTO
                {
                    Id = notDeletedId.ToString(),
                    Error = errorMessage,
                    Details = $"Medication lot với ID {notDeletedId} {errorMessage.ToLower()}"
                });
            }
        }

        private string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount} lô thuốc";
            }
            else if (result.IsCompleteFailure)
            {
                return $"Không thể {operation} bất kỳ lô thuốc nào. {result.FailureCount} lỗi";
            }
            else if (result.IsPartialSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} lô thuốc. {result.FailureCount} lỗi";
            }
            else
            {
                return $"Không có lô thuốc nào được {operation}";
            }
        }

        private async Task<(bool IsSuccess, string Message)> ValidateCreateRequestAsync(CreateMedicationLotRequest request)
        {
            var medication = await _unitOfWork.MedicationRepository.GetByIdAsync(request.MedicationId);
            if (medication == null)
            {
                return (false, "Không tìm thấy thuốc");
            }

            var lotNumberExists = await _unitOfWork.MedicationLotRepository.LotNumberExistsAsync(request.LotNumber);
            if (lotNumberExists)
            {
                return (false, $"Số lô '{request.LotNumber}' đã tồn tại");
            }

            if (request.ExpiryDate.Date <= DateTime.UtcNow.Date)
            {
                return (false, "Ngày hết hạn phải lớn hơn ngày hiện tại");
            }

            if (request.Quantity <= 0)
            {
                return (false, "Số lượng phải lớn hơn 0");
            }

            return (true, "Valid");
        }

        private async Task<(bool IsSuccess, string Message)> ValidateUpdateRequestAsync(UpdateMedicationLotRequest request, Guid excludeId)
        {
            var lotNumberExists = await _unitOfWork.MedicationLotRepository.LotNumberExistsAsync(request.LotNumber, excludeId);
            if (lotNumberExists)
            {
                return (false, $"Số lô '{request.LotNumber}' đã tồn tại");
            }

            if (request.Quantity <= 0)
            {
                return (false, "Số lượng phải lớn hơn 0");
            }

            return (true, "Valid");
        }

        #endregion
    }
}