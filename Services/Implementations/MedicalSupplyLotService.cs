using Microsoft.Extensions.Logging;
using System.Data;

namespace Services.Implementations
{
    public class MedicalSupplyLotService : BaseService<MedicalSupplyLot, Guid>, IMedicalSupplyLotService
    {
        private readonly ILogger<MedicalSupplyLotService> _logger;

        public MedicalSupplyLotService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICurrentTime currentTime,
            ILogger<MedicalSupplyLotService> logger)
            : base(unitOfWork.MedicalSupplyLotRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<MedicalSupplyLotResponseDTO>>> GetMedicalSupplyLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicalSupplyId = null, bool? isExpired = null, bool includeDeleted = false)
        {
            try
            {
                var lots = await _unitOfWork.MedicalSupplyLotRepository.GetMedicalSupplyLotsAsync(
                    pageNumber, pageSize, searchTerm, medicalSupplyId, isExpired, includeDeleted);

                var dtos = lots.Select(MedicalSupplyLotMapper.ToResponseDTO);
                var result = MedicalSupplyLotMapper.ToPagedResult(lots, dtos);

                return ApiResult<PagedList<MedicalSupplyLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô vật tư y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply lots");
                return ApiResult<PagedList<MedicalSupplyLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicalSupplyLotResponseDTO>> GetMedicalSupplyLotByIdAsync(Guid id)
        {
            try
            {
                var lot = await _unitOfWork.MedicalSupplyLotRepository.GetLotWithSupplyAsync(id);
                if (lot == null)
                    return ApiResult<MedicalSupplyLotResponseDTO>.Failure(new Exception("Không tìm thấy lô vật tư y tế"));

                var dto = MedicalSupplyLotMapper.ToResponseDTO(lot);
                return ApiResult<MedicalSupplyLotResponseDTO>.Success(dto, "Lấy thông tin lô vật tư y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply lot by ID: {LotId}", id);
                return ApiResult<MedicalSupplyLotResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicalSupplyLotResponseDTO>> CreateMedicalSupplyLotAsync(CreateMedicalSupplyLotRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validation = await ValidateCreateRequestAsync(request);
                    if (!validation.IsSuccess)
                        return ApiResult<MedicalSupplyLotResponseDTO>.Failure(new Exception(validation.Message));

                    var entity = MedicalSupplyLotMapper.MapFromCreateRequest(request);
                    var created = await CreateAsync(entity);

                    await UpdateSupplyCurrentStockAsync(request.MedicalSupplyId);

                    var lot = await _unitOfWork.MedicalSupplyLotRepository.GetLotWithSupplyAsync(created.Id)
                              ?? throw new Exception("Không thể lấy lô vừa tạo");
                    var dto = MedicalSupplyLotMapper.ToResponseDTO(lot);

                    return ApiResult<MedicalSupplyLotResponseDTO>.Success(dto, "Tạo lô vật tư y tế thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating medical supply lot");
                    return ApiResult<MedicalSupplyLotResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<MedicalSupplyLotResponseDTO>> UpdateMedicalSupplyLotAsync(
            Guid id, UpdateMedicalSupplyLotRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var lot = await _unitOfWork.MedicalSupplyLotRepository.GetByIdAsync(id);
                    if (lot == null)
                        return ApiResult<MedicalSupplyLotResponseDTO>.Failure(new Exception("Không tìm thấy lô vật tư y tế"));

                    var validation = await ValidateUpdateRequestAsync(request, id);
                    if (!validation.IsSuccess)
                        return ApiResult<MedicalSupplyLotResponseDTO>.Failure(new Exception(validation.Message));

                    var oldSupplyId = lot.MedicalSupplyId;
                    MedicalSupplyLotMapper.MapFromUpdateRequest(request, lot);
                    await UpdateAsync(lot);

                    // Update stock for the affected supply
                    await UpdateSupplyCurrentStockAsync(oldSupplyId);

                    var updated = await _unitOfWork.MedicalSupplyLotRepository.GetLotWithSupplyAsync(id)
                                   ?? throw new Exception("Không thể lấy lô đã cập nhật");
                    var dto = MedicalSupplyLotMapper.ToResponseDTO(updated);

                    return ApiResult<MedicalSupplyLotResponseDTO>.Success(dto, "Cập nhật lô vật tư y tế thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating medical supply lot: {LotId}", id);
                    return ApiResult<MedicalSupplyLotResponseDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Unified Delete & Restore Operations

        /// <summary>
        /// Xóa lô vật tư y tế (hỗ trợ cả xóa mềm và xóa vĩnh viễn)
        /// </summary>
        public async Task<ApiResult<BatchOperationResultDTO>> DeleteMedicalSupplyLotsAsync(List<Guid> ids, bool isPermanent = false)
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

                    var operationType = isPermanent ? "xóa vĩnh viễn" : "xóa";
                    _logger.LogInformation("Starting batch {Operation} for {Count} medical supply lots",
                        operationType, ids.Count);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    if (isPermanent)
                    {
                        // Permanent delete - get all lots (including deleted ones)
                        var allLots = await _unitOfWork.MedicalSupplyLotRepository.GetMedicalSupplyLotsByIdsAsync(ids, includeDeleted: true);
                        var existingIds = allLots.Select(l => l.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        // Add errors for not found lots
                        foreach (var notFoundId in notFoundIds)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = notFoundId.ToString(),
                                Error = "Không tìm thấy lô vật tư y tế",
                                Details = $"Medical supply lot với ID {notFoundId} không tồn tại"
                            });
                        }

                        if (existingIds.Any())
                        {
                            // Store affected supply IDs before deletion
                            var affectedSupplyIds = allLots.Select(l => l.MedicalSupplyId).Distinct().ToList();

                            var deletedCount = await _unitOfWork.MedicalSupplyLotRepository.PermanentDeleteLotsAsync(existingIds);

                            if (deletedCount > 0)
                            {
                                // Update stock for all affected supplies
                                foreach (var supplyId in affectedSupplyIds)
                                {
                                    await UpdateSupplyCurrentStockAsync(supplyId);
                                }

                                result.SuccessCount = deletedCount;
                                result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                            }
                        }
                    }
                    else
                    {
                        // Soft delete - only get non-deleted lots
                        var existingLots = await _unitOfWork.MedicalSupplyLotRepository.GetMedicalSupplyLotsByIdsAsync(ids, includeDeleted: false);
                        var existingIds = existingLots.Select(l => l.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        // Add errors for not found or already deleted lots
                        foreach (var notFoundId in notFoundIds)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = notFoundId.ToString(),
                                Error = "Không tìm thấy lô vật tư y tế hoặc đã bị xóa",
                                Details = $"Medical supply lot với ID {notFoundId} không tồn tại hoặc đã bị xóa"
                            });
                        }

                        if (existingIds.Any())
                        {
                            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                            var deletedCount = await _unitOfWork.MedicalSupplyLotRepository.SoftDeleteLotsAsync(existingIds, currentUserId);

                            if (deletedCount > 0)
                            {
                                // Update stock for all affected supplies
                                var affectedSupplyIds = existingLots.Select(l => l.MedicalSupplyId).Distinct();
                                foreach (var supplyId in affectedSupplyIds)
                                {
                                    await UpdateSupplyCurrentStockAsync(supplyId);
                                }

                                result.SuccessCount = deletedCount;
                                result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                            }
                        }
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage(operationType, result);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch delete of medical supply lots (isPermanent: {IsPermanent})", isPermanent);
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreMedicalSupplyLotsAsync(List<Guid> ids)
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

                    _logger.LogInformation("Starting batch restore for {Count} medical supply lots", ids.Count);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    var deletedLots = await _unitOfWork.MedicalSupplyLotRepository.GetMedicalSupplyLotsByIdsAsync(ids, includeDeleted: true);
                    var deletedLotIds = deletedLots.Where(l => l.IsDeleted).Select(l => l.Id).ToList();
                    var notDeletedIds = ids.Except(deletedLotIds).ToList();

                    foreach (var notDeletedId in notDeletedIds)
                    {
                        var lot = deletedLots.FirstOrDefault(l => l.Id == notDeletedId);
                        string errorMessage = lot == null
                            ? "Không tìm thấy lô vật tư y tế"
                            : "Lô vật tư y tế chưa bị xóa";

                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = notDeletedId.ToString(),
                            Error = errorMessage,
                            Details = $"Medical supply lot với ID {notDeletedId} {errorMessage.ToLower()}"
                        });
                    }

                    if (deletedLotIds.Any())
                    {
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var restoredCount = await _unitOfWork.MedicalSupplyLotRepository.RestoreLotsAsync(deletedLotIds, currentUserId);

                        if (restoredCount > 0)
                        {
                            // Update stock for all affected supplies
                            var affectedSupplyIds = deletedLots.Where(l => deletedLotIds.Contains(l.Id))
                                                             .Select(l => l.MedicalSupplyId).Distinct();
                            foreach (var supplyId in affectedSupplyIds)
                            {
                                await UpdateSupplyCurrentStockAsync(supplyId);
                            }

                            result.SuccessCount = restoredCount;
                            result.SuccessIds = deletedLotIds.Select(id => id.ToString()).ToList();
                        }
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch restore of medical supply lots");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<MedicalSupplyLotResponseDTO>>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var lots = await _unitOfWork.MedicalSupplyLotRepository.GetSoftDeletedLotsAsync(
                    pageNumber, pageSize, searchTerm);

                var lotDTOs = lots.Select(MedicalSupplyLotMapper.ToResponseDTO).ToList();
                var result = CreatePagedResult(lots, lotDTOs);

                return ApiResult<PagedList<MedicalSupplyLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô vật tư y tế đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted lots");
                return ApiResult<PagedList<MedicalSupplyLotResponseDTO>>.Failure(ex);
            }
        }

        #endregion

        #region Business Logic Operations

        public async Task<ApiResult<List<MedicalSupplyLotResponseDTO>>> GetExpiringLotsAsync(int daysBeforeExpiry = 30)
        {
            try
            {
                var lots = await _unitOfWork.MedicalSupplyLotRepository.GetExpiringLotsAsync(daysBeforeExpiry);
                var lotDTOs = lots.Select(MedicalSupplyLotMapper.ToResponseDTO).ToList();

                return ApiResult<List<MedicalSupplyLotResponseDTO>>.Success(
                    lotDTOs, $"Lấy danh sách lô vật tư y tế sắp hết hạn trong {daysBeforeExpiry} ngày thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring lots");
                return ApiResult<List<MedicalSupplyLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicalSupplyLotResponseDTO>>> GetExpiredLotsAsync()
        {
            try
            {
                var lots = await _unitOfWork.MedicalSupplyLotRepository.GetExpiredLotsAsync();
                var lotDTOs = lots.Select(MedicalSupplyLotMapper.ToResponseDTO).ToList();

                return ApiResult<List<MedicalSupplyLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô vật tư y tế đã hết hạn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired lots");
                return ApiResult<List<MedicalSupplyLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicalSupplyLotResponseDTO>>> GetLotsByMedicalSupplyIdAsync(Guid medicalSupplyId)
        {
            try
            {
                var lots = await _unitOfWork.MedicalSupplyLotRepository.GetLotsByMedicalSupplyIdAsync(medicalSupplyId);
                var lotDTOs = lots.Select(MedicalSupplyLotMapper.ToResponseDTO).ToList();

                return ApiResult<List<MedicalSupplyLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô vật tư y tế theo ID vật tư thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lots by medical supply ID: {SupplyId}", medicalSupplyId);
                return ApiResult<List<MedicalSupplyLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<int>> GetAvailableQuantityAsync(Guid medicalSupplyId)
        {
            try
            {
                var quantity = await _unitOfWork.MedicalSupplyLotRepository.GetAvailableQuantityAsync(medicalSupplyId);
                return ApiResult<int>.Success(quantity, "Lấy số lượng khả dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available quantity for medical supply: {SupplyId}", medicalSupplyId);
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

                    var lot = await _unitOfWork.MedicalSupplyLotRepository.GetByIdAsync(lotId);
                    if (lot == null)
                    {
                        return ApiResult<bool>.Failure(
                            new Exception("Không tìm thấy lô vật tư y tế"));
                    }

                    var supplyId = lot.MedicalSupplyId;
                    var success = await _unitOfWork.MedicalSupplyLotRepository.UpdateQuantityAsync(lotId, newQuantity);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new Exception("Không thể cập nhật số lượng"));
                    }

                    await UpdateSupplyCurrentStockAsync(supplyId);

                    return ApiResult<bool>.Success(true, "Cập nhật số lượng thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating quantity for lot: {LotId}", lotId);
                    return ApiResult<bool>.Failure(ex);
                }
            });
        }

        #endregion

        #region Private Helper Methods

        private string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount} lô vật tư y tế";
            }
            else if (result.IsCompleteFailure)
            {
                return $"Không thể {operation} bất kỳ lô vật tư y tế nào. {result.FailureCount} lỗi";
            }
            else if (result.IsPartialSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} lô vật tư y tế. {result.FailureCount} lỗi";
            }
            else
            {
                return $"Không có lô vật tư y tế nào được {operation}";
            }
        }

        private async Task<(bool IsSuccess, string Message)> ValidateCreateRequestAsync(CreateMedicalSupplyLotRequest request)
        {
            var supply = await _unitOfWork.MedicalSupplyRepository.GetByIdAsync(request.MedicalSupplyId);
            if (supply == null)
            {
                return (false, "Không tìm thấy vật tư y tế");
            }

            var lotNumberExists = await _unitOfWork.MedicalSupplyLotRepository.LotNumberExistsAsync(request.LotNumber);
            if (lotNumberExists)
            {
                return (false, $"Số lô '{request.LotNumber}' đã tồn tại");
            }

            if (request.ExpirationDate.Date <= DateTime.UtcNow.Date)
            {
                return (false, "Ngày hết hạn phải lớn hơn ngày hiện tại");
            }

            if (request.ManufactureDate.Date > DateTime.UtcNow.Date)
            {
                return (false, "Ngày sản xuất không được lớn hơn ngày hiện tại");
            }

            if (request.ManufactureDate.Date >= request.ExpirationDate.Date)
            {
                return (false, "Ngày sản xuất phải nhỏ hơn ngày hết hạn");
            }

            if (request.Quantity <= 0)
            {
                return (false, "Số lượng phải lớn hơn 0");
            }

            return (true, "Valid");
        }

        private async Task<(bool IsSuccess, string Message)> ValidateUpdateRequestAsync(UpdateMedicalSupplyLotRequest request, Guid excludeId)
        {
            var lotNumberExists = await _unitOfWork.MedicalSupplyLotRepository.LotNumberExistsAsync(request.LotNumber, excludeId);
            if (lotNumberExists)
            {
                return (false, $"Số lô '{request.LotNumber}' đã tồn tại");
            }

            if (request.ManufactureDate.Date > DateTime.UtcNow.Date)
            {
                return (false, "Ngày sản xuất không được lớn hơn ngày hiện tại");
            }

            if (request.ManufactureDate.Date >= request.ExpirationDate.Date)
            {
                return (false, "Ngày sản xuất phải nhỏ hơn ngày hết hạn");
            }

            if (request.Quantity < 0)
            {
                return (false, "Số lượng không được âm");
            }

            return (true, "Valid");
        }

        private async Task UpdateSupplyCurrentStockAsync(Guid medicalSupplyId)
        {
            try
            {
                var currentStock = await _unitOfWork.MedicalSupplyLotRepository.CalculateCurrentStockForSupplyAsync(medicalSupplyId);
                await _unitOfWork.MedicalSupplyRepository.UpdateCurrentStockAsync(medicalSupplyId, currentStock);
                // Note: SaveChanges is handled by the transaction extension
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current stock for supply: {SupplyId}", medicalSupplyId);
                throw; // Re-throw to ensure transaction rollback
            }
        }

        private static PagedList<MedicalSupplyLotResponseDTO> CreatePagedResult(PagedList<MedicalSupplyLot> sourcePaged, List<MedicalSupplyLotResponseDTO> mappedItems)
        {
            var meta = sourcePaged.MetaData;
            return new PagedList<MedicalSupplyLotResponseDTO>(
                mappedItems,
                meta.TotalCount,
                meta.CurrentPage,
                meta.PageSize);
        }

        #endregion
    }
}