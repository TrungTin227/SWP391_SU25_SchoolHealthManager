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
                    return ApiResult<MedicalSupplyLotResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy lô vật tư y tế"));

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
                        return ApiResult<MedicalSupplyLotResponseDTO>.Failure(
                            new ArgumentException(validation.Message));

                    var entity = MedicalSupplyLotMapper.MapFromCreateRequest(request);

                    // BaseService sẽ tự động xử lý audit fields
                    var created = await CreateAsync(entity);

                    await UpdateSupplyCurrentStockAsync(request.MedicalSupplyId);

                    var lot = await _unitOfWork.MedicalSupplyLotRepository.GetLotWithSupplyAsync(created.Id)
                              ?? throw new InvalidOperationException("Không thể lấy lô vừa tạo");
                    var dto = MedicalSupplyLotMapper.ToResponseDTO(lot);

                    _logger.LogInformation("Created medical supply lot: {LotId} for supply: {SupplyId}",
                        created.Id, request.MedicalSupplyId);

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
                        return ApiResult<MedicalSupplyLotResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy lô vật tư y tế"));

                    var validation = await ValidateUpdateRequestAsync(request, id);
                    if (!validation.IsSuccess)
                        return ApiResult<MedicalSupplyLotResponseDTO>.Failure(
                            new ArgumentException(validation.Message));

                    var oldSupplyId = lot.MedicalSupplyId;
                    MedicalSupplyLotMapper.MapFromUpdateRequest(request, lot);

                    // BaseService sẽ tự động xử lý audit fields
                    await UpdateAsync(lot);

                    // Update stock for the affected supply
                    await UpdateSupplyCurrentStockAsync(oldSupplyId);

                    var updated = await _unitOfWork.MedicalSupplyLotRepository.GetLotWithSupplyAsync(id)
                                   ?? throw new InvalidOperationException("Không thể lấy lô đã cập nhật");
                    var dto = MedicalSupplyLotMapper.ToResponseDTO(updated);

                    _logger.LogInformation("Updated medical supply lot: {LotId}", id);

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
                        await ProcessPermanentDelete(ids, result);
                    }
                    else
                    {
                        await ProcessSoftDelete(ids, result);
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage(operationType, result);

                    _logger.LogInformation("Batch {Operation} completed: {SuccessCount}/{TotalCount} successful",
                        operationType, result.SuccessCount, result.TotalRequested);

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

                    // Add errors for lots that are not deleted or don't exist
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

                    // Process valid restorations
                    if (deletedLotIds.Any())
                    {
                        var currentUserId = GetCurrentUserIdOrThrow();
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

                            _logger.LogInformation("Restored {Count} medical supply lots", restoredCount);
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
                            new ArgumentException("Số lượng không được âm"));
                    }

                    var lot = await _unitOfWork.MedicalSupplyLotRepository.GetByIdAsync(lotId);
                    if (lot == null)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException("Không tìm thấy lô vật tư y tế"));
                    }

                    var supplyId = lot.MedicalSupplyId;
                    var success = await _unitOfWork.MedicalSupplyLotRepository.UpdateQuantityAsync(lotId, newQuantity);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new InvalidOperationException("Không thể cập nhật số lượng"));
                    }

                    await UpdateSupplyCurrentStockAsync(supplyId);

                    _logger.LogInformation("Updated quantity for lot {LotId} to {Quantity}", lotId, newQuantity);

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

        /// <summary>
        /// Lấy current user ID và ném exception nếu null
        /// </summary>
        private Guid GetCurrentUserIdOrThrow()
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng hiện tại");
            }
            return currentUserId.Value;
        }

        private async Task ProcessPermanentDelete(List<Guid> ids, BatchOperationResultDTO result)
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

                    _logger.LogInformation("Permanently deleted {Count} medical supply lots", deletedCount);
                }
            }
        }

        private async Task ProcessSoftDelete(List<Guid> ids, BatchOperationResultDTO result)
        {
            foreach (var id in ids)
            {
                try
                {
                    var lot = await _unitOfWork.MedicalSupplyLotRepository.GetByIdAsync(id);
                    if (lot == null)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Không tìm thấy lô vật tư y tế",
                            Details = $"Medical supply lot với ID {id} không tồn tại"
                        });
                        continue;
                    }

                    if (lot.IsDeleted)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Đã bị xóa",
                            Details = "Lô vật tư y tế đã được xóa trước đó"
                        });
                        continue;
                    }

                    var supplyId = lot.MedicalSupplyId;

                    // Sử dụng BaseService DeleteAsync
                    var deleteResult = await DeleteAsync(id);
                    if (deleteResult)
                    {
                        await UpdateSupplyCurrentStockAsync(supplyId);

                        result.SuccessIds.Add(id.ToString());
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Xóa thất bại",
                            Details = "Không thể xóa lô vật tư y tế"
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new BatchOperationErrorDTO
                    {
                        Id = id.ToString(),
                        Error = "Lỗi hệ thống",
                        Details = ex.Message
                    });
                }
            }
        }

        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công {result.SuccessCount} lô vật tư y tế";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} lô vật tư y tế. " +
                       $"{result.FailureCount} thất bại";

            return $"Không thể {operation} lô vật tư y tế nào. Tất cả {result.FailureCount} yêu cầu đều thất bại";
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

            var currentDate = _currentTime.GetVietnamTime().Date;

            if (request.ExpirationDate.Date <= currentDate)
            {
                return (false, "Ngày hết hạn phải lớn hơn ngày hiện tại");
            }

            if (request.ManufactureDate.Date > currentDate)
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

            var currentDate = _currentTime.GetVietnamTime().Date;

            if (request.ManufactureDate.Date > currentDate)
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

        private static PagedList<MedicalSupplyLotResponseDTO> CreatePagedResult(
            PagedList<MedicalSupplyLot> sourcePaged,
            List<MedicalSupplyLotResponseDTO> mappedItems)
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