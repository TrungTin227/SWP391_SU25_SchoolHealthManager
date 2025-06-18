using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Commons;
using Services.Mappers;
using System.Data;

namespace Services.Implementations
{
    public class MedicationLotService : BaseService<MedicationLot, Guid>, IMedicationLotService
    {
        private readonly IMedicationLotRepository _medicationLotRepository;
        private readonly IMedicationRepository _medicationRepository;
        private readonly ILogger<MedicationLotService> _logger;

        public MedicationLotService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<MedicationLotService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.MedicationLotRepository, currentUserService, unitOfWork, currentTime)
        {
            _medicationLotRepository = unitOfWork.MedicationLotRepository;
            _medicationRepository = unitOfWork.MedicationRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetMedicationLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicationId = null, bool? isExpired = null)
        {
            try
            {
                var lots = await _medicationLotRepository.GetMedicationLotsAsync(
                    pageNumber, pageSize, searchTerm, medicationId, isExpired);

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
                var lot = await _medicationLotRepository.GetLotWithMedicationAsync(id);
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
                    var createdLot = await CreateAsync(lot);

                    var lotWithMedication = await _medicationLotRepository.GetLotWithMedicationAsync(createdLot.Id);
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
                    var lot = await _medicationLotRepository.GetByIdAsync(id);
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
                    await UpdateAsync(lot);

                    var updatedLot = await _medicationLotRepository.GetLotWithMedicationAsync(id);
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

        #endregion

        #region Batch Operations (Unified - Support Single and Multiple)

        /// <summary>
        /// Xóa một hoặc nhiều lô thuốc cùng lúc (soft delete hoặc permanent delete)
        /// Hỗ trợ cả single (1 ID) và batch (nhiều IDs)
        /// </summary>
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

                    var operationType = isPermanent ? "permanent delete" : "soft delete";
                    var operationText = isPermanent ? "xóa vĩnh viễn" : "xóa";

                    _logger.LogInformation("Starting batch {OperationType} for {Count} medication lots",
                        operationType, ids.Count);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    if (isPermanent)
                    {
                        // Permanent delete: lấy tất cả lots (bao gồm cả đã xóa)
                        var allLots = await _medicationLotRepository.GetMedicationLotsByIdsAsync(ids, includeDeleted: true);
                        var existingIds = allLots.Select(l => l.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        // Thêm lỗi cho các ID không tìm thấy
                        foreach (var notFoundId in notFoundIds)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = notFoundId.ToString(),
                                Error = "Không tìm thấy lô thuốc",
                                Details = $"Medication lot với ID {notFoundId} không tồn tại"
                            });
                        }

                        // Thực hiện permanent delete cho các lots tồn tại
                        if (existingIds.Any())
                        {
                            var deletedCount = await _medicationLotRepository.PermanentDeleteLotsAsync(existingIds);

                            if (deletedCount > 0)
                            {
                                await _unitOfWork.SaveChangesAsync();

                                result.SuccessCount = deletedCount;
                                result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                            }
                        }
                    }
                    else
                    {
                        // Soft delete: chỉ lấy các lots chưa bị xóa
                        var existingLots = await _medicationLotRepository.GetMedicationLotsByIdsAsync(ids, includeDeleted: false);
                        var existingIds = existingLots.Select(l => l.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        // Thêm lỗi cho các ID không tìm thấy
                        foreach (var notFoundId in notFoundIds)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = notFoundId.ToString(),
                                Error = "Không tìm thấy lô thuốc hoặc đã bị xóa",
                                Details = $"Medication lot với ID {notFoundId} không tồn tại hoặc đã bị xóa"
                            });
                        }

                        // Thực hiện soft delete cho các lots tồn tại
                        if (existingIds.Any())
                        {
                            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                            var deletedCount = await _medicationLotRepository.SoftDeleteLotsAsync(existingIds, currentUserId);

                            if (deletedCount > 0)
                            {
                                await _unitOfWork.SaveChangesAsync();

                                result.SuccessCount = deletedCount;
                                result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                            }
                        }
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage(operationText, result);

                    _logger.LogInformation(
                        "Batch {OperationType} completed: {SuccessCount} success, {FailureCount} failures",
                        operationType, result.SuccessCount, result.FailureCount);

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

        /// <summary>
        /// Khôi phục một hoặc nhiều lô thuốc cùng lúc
        /// Hỗ trợ cả single (1 ID) và batch (nhiều IDs)
        /// </summary>
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

                    // Lấy danh sách các lots đã bị soft delete
                    var deletedLots = await _medicationLotRepository.GetMedicationLotsByIdsAsync(ids, includeDeleted: true);
                    var deletedLotIds = deletedLots.Where(l => l.IsDeleted).Select(l => l.Id).ToList();
                    var notDeletedIds = ids.Except(deletedLotIds).ToList();

                    // Thêm lỗi cho các ID không phải là soft deleted
                    foreach (var notDeletedId in notDeletedIds)
                    {
                        var lot = deletedLots.FirstOrDefault(l => l.Id == notDeletedId);
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

                    // Thực hiện restore cho các lots đã bị soft delete
                    if (deletedLotIds.Any())
                    {
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var restoredCount = await _medicationLotRepository.RestoreLotsAsync(deletedLotIds, currentUserId);

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

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var lots = await _medicationLotRepository.GetSoftDeletedLotsAsync(
                    pageNumber, pageSize, searchTerm);

                var result = MedicationLotMapper.MapToPagedResponseDTO(lots);

                return ApiResult<PagedList<MedicationLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô thuốc đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted lots");
                return ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<BatchOperationResultDTO>> CleanupExpiredLotsAsync(int daysToExpire = 90)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var deletedCount = await _medicationLotRepository.PermanentDeleteExpiredLotsAsync(daysToExpire);
                    await _unitOfWork.SaveChangesAsync();

                    var batchResult = new BatchOperationResultDTO
                    {
                        TotalRequested = deletedCount,
                        SuccessCount = deletedCount,
                        FailureCount = 0,
                        Message = $"Đã dọn dẹp {deletedCount} lô thuốc hết hạn"
                    };

                    return ApiResult<BatchOperationResultDTO>.Success(
                        batchResult,
                        batchResult.Message
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up expired lots");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Business Logic Operations

        public async Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiringLotsAsync(int daysBeforeExpiry = 30)
        {
            try
            {
                var lots = await _medicationLotRepository.GetExpiringLotsAsync(daysBeforeExpiry);
                var lotDTOs = MedicationLotMapper.MapToResponseDTOList(lots);

                return ApiResult<List<MedicationLotResponseDTO>>.Success(
                    lotDTOs, $"Lấy danh sách lô thuốc sắp hết hạn trong {daysBeforeExpiry} ngày thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring lots");
                return ApiResult<List<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiredLotsAsync()
        {
            try
            {
                var lots = await _medicationLotRepository.GetExpiredLotsAsync();
                var lotDTOs = MedicationLotMapper.MapToResponseDTOList(lots);

                return ApiResult<List<MedicationLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô thuốc đã hết hạn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired lots");
                return ApiResult<List<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationLotResponseDTO>>> GetLotsByMedicationIdAsync(Guid medicationId)
        {
            try
            {
                var lots = await _medicationLotRepository.GetLotsByMedicationIdAsync(medicationId);
                var lotDTOs = MedicationLotMapper.MapToResponseDTOList(lots);

                return ApiResult<List<MedicationLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô thuốc theo ID thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lots by medication ID: {MedicationId}", medicationId);
                return ApiResult<List<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<int>> GetAvailableQuantityAsync(Guid medicationId)
        {
            try
            {
                var quantity = await _medicationLotRepository.GetAvailableQuantityAsync(medicationId);
                return ApiResult<int>.Success(quantity, "Lấy số lượng khả dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available quantity for medication: {MedicationId}", medicationId);
                return ApiResult<int>.Failure(ex);
            }
        }

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

                    var success = await _medicationLotRepository.UpdateQuantityAsync(lotId, newQuantity);
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

                var statistics = await _medicationLotRepository.GetAllStatisticsAsync(currentDate, expiryThreshold);

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
            var medication = await _medicationRepository.GetByIdAsync(request.MedicationId);
            if (medication == null)
            {
                return (false, "Không tìm thấy thuốc");
            }

            var lotNumberExists = await _medicationLotRepository.LotNumberExistsAsync(request.LotNumber);
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
            var lotNumberExists = await _medicationLotRepository.LotNumberExistsAsync(request.LotNumber, excludeId);
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