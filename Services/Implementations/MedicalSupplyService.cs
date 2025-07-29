using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Services.Implementations
{
    public class MedicalSupplyService : BaseService<MedicalSupply, Guid>, IMedicalSupplyService
    {
        private readonly ILogger<MedicalSupplyService> _logger;

        public MedicalSupplyService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICurrentTime currentTime,
            ILogger<MedicalSupplyService> logger)
            : base(unitOfWork.MedicalSupplyRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<MedicalSupplyResponseDTO>>> GetMedicalSuppliesAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm = null,
    bool? isActive = null,
    bool includeDeleted = false)
        {
            try
            {
                // 1. Lấy dữ liệu đã bao gồm 'Lots' từ repository
                var pagedSupplies = await _unitOfWork.MedicalSupplyRepository.GetMedicalSuppliesAsync(
                    pageNumber, pageSize, searchTerm, isActive, includeDeleted);

                // 2. Map thẳng từ entity sang DTO.
                var dtoList = pagedSupplies.Select(MedicalSupplyMapper.MapToResponseDTO).ToList();

                // 3. Lấy tổng số bản ghi (TotalCount) từ repository
                var totalCount = await _unitOfWork.MedicalSupplyRepository.CountAsync(s =>
                    (string.IsNullOrEmpty(searchTerm) || s.Name.Contains(searchTerm)) &&
                    (!isActive.HasValue || s.IsActive == isActive.Value) &&
                    (includeDeleted || !s.IsDeleted)
                );
                // 4. Tạo kết quả PagedList cho DTO
                var result = new PagedList<MedicalSupplyResponseDTO>(
                    dtoList,
                    totalCount,
                    pageNumber,
                    pageSize
                );

                return ApiResult<PagedList<MedicalSupplyResponseDTO>>.Success(
                    result, "Lấy danh sách vật tư y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supplies");
                return ApiResult<PagedList<MedicalSupplyResponseDTO>>.Failure(ex);
            }
        }


        public async Task<ApiResult<MedicalSupplyResponseDTO>> GetMedicalSupplyByIdAsync(Guid id)
        {
            try
            {
                var supply = await _unitOfWork.MedicalSupplyRepository.GetByIdAsync(id);
                if (supply == null)
                {
                    return ApiResult<MedicalSupplyResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy vật tư y tế"));
                }

                var supplyDTO = MedicalSupplyMapper.MapToResponseDTO(supply);
                return ApiResult<MedicalSupplyResponseDTO>.Success(
                    supplyDTO, "Lấy thông tin vật tư y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply by ID: {SupplyId}", id);
                return ApiResult<MedicalSupplyResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicalSupplyDetailResponseDTO>> GetMedicalSupplyDetailByIdAsync(Guid id)
        {
            try
            {
                var supply = await _unitOfWork.MedicalSupplyRepository.GetSupplyWithLotsAsync(id);
                if (supply == null)
                {
                    return ApiResult<MedicalSupplyDetailResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy vật tư y tế"));
                }

                var supplyDetailDTO = MedicalSupplyMapper.MapToDetailResponseDTO(supply);
                return ApiResult<MedicalSupplyDetailResponseDTO>.Success(
                    supplyDetailDTO, "Lấy chi tiết vật tư y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply detail by ID: {SupplyId}", id);
                return ApiResult<MedicalSupplyDetailResponseDTO>.Failure(ex);
            }
        }


        public async Task<ApiResult<MedicalSupplyResponseDTO>> CreateMedicalSupplyAsync(CreateMedicalSupplyRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = await ValidateCreateRequestAsync(request);
                    if (!validationResult.IsSuccess)
                    {
                        return ApiResult<MedicalSupplyResponseDTO>.Failure(
                            new ArgumentException(validationResult.Message));
                    }

                    var supply = MedicalSupplyMapper.MapFromCreateRequest(request);
                    var createdSupply = await CreateAsync(supply);
                    var supplyDTO = MedicalSupplyMapper.MapToResponseDTO(createdSupply);

                    _logger.LogInformation("Created medical supply: {SupplyId} with name: {Name}",
                        createdSupply.Id, createdSupply.Name);

                    return ApiResult<MedicalSupplyResponseDTO>.Success(
                        supplyDTO, "Tạo vật tư y tế thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating medical supply");
                    return ApiResult<MedicalSupplyResponseDTO>.Failure(ex);
                }
            });
        }


        public async Task<ApiResult<MedicalSupplyResponseDTO>> UpdateMedicalSupplyAsync(
    Guid id, UpdateMedicalSupplyRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var supply = await _unitOfWork.MedicalSupplyRepository.GetByIdAsync(id);
                    if (supply == null)
                    {
                        return ApiResult<MedicalSupplyResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy vật tư y tế"));
                    }

                    var validationResult = await ValidateUpdateRequestAsync(request, id);
                    if (!validationResult.IsSuccess)
                    {
                        return ApiResult<MedicalSupplyResponseDTO>.Failure(
                            new ArgumentException(validationResult.Message));
                    }

                    MedicalSupplyMapper.MapFromUpdateRequest(request, supply);
                    await UpdateAsync(supply);
                    var supplyDTO = MedicalSupplyMapper.MapToResponseDTO(supply);

                    _logger.LogInformation("Updated medical supply: {SupplyId}", id);
                    return ApiResult<MedicalSupplyResponseDTO>.Success(
                        supplyDTO, "Cập nhật vật tư y tế thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating medical supply: {SupplyId}", id);
                    return ApiResult<MedicalSupplyResponseDTO>.Failure(ex);
                }
            });
        }


        #endregion

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<MedicalSupplyResponseDTO>>> GetSoftDeletedSuppliesAsync(
    int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var supplies = await _unitOfWork.MedicalSupplyRepository.GetSoftDeletedSuppliesAsync(
                    pageNumber, pageSize, searchTerm);

                var supplyDTOs = supplies.Select(MedicalSupplyMapper.MapToResponseDTO).ToList();
                var result = MedicalSupplyMapper.CreatePagedResult(supplies, supplyDTOs);

                return ApiResult<PagedList<MedicalSupplyResponseDTO>>.Success(
                    result, "Lấy danh sách vật tư y tế đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted supplies");
                return ApiResult<PagedList<MedicalSupplyResponseDTO>>.Failure(ex);
            }
        }

        #endregion

        #region Unified Delete & Restore Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteMedicalSuppliesAsync(List<Guid> ids, bool isPermanent = false)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate input
                    var validationResult = ValidateBatchInput(ids, isPermanent ? "xóa vĩnh viễn" : "xóa");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    var operationType = isPermanent ? "xóa vĩnh viễn" : "xóa";
                    _logger.LogInformation("Starting batch {Operation} for {Count} medical supplies",
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
                    _logger.LogError(ex, "Error during batch delete of medical supplies (isPermanent: {IsPermanent})", isPermanent);
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreMedicalSuppliesAsync(List<Guid> ids)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate input
                    var validationResult = ValidateBatchInput(ids, "khôi phục");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    _logger.LogInformation("Starting batch restore for {Count} medical supplies", ids.Count);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    // Get deleted supplies
                    var deletedSupplies = await _unitOfWork.MedicalSupplyRepository.GetMedicalSuppliesByIdsAsync(ids, includeDeleted: true);
                    var deletedSupplyIds = deletedSupplies.Where(s => s.IsDeleted).Select(s => s.Id).ToList();
                    var notDeletedIds = ids.Except(deletedSupplyIds).ToList();

                    // Add errors for inappropriate IDs
                    foreach (var notDeletedId in notDeletedIds)
                    {
                        var supply = deletedSupplies.FirstOrDefault(s => s.Id == notDeletedId);
                        string errorMessage = supply == null
                            ? "Không tìm thấy vật tư y tế"
                            : "Vật tư y tế chưa bị xóa";

                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = notDeletedId.ToString(),
                            Error = errorMessage,
                            Details = $"Medical supply với ID {notDeletedId} {errorMessage.ToLower()}"
                        });
                    }

                    // Execute restore for valid supplies
                    if (deletedSupplyIds.Any())
                    {
                        var currentUserId = GetCurrentUserIdOrThrow();
                        var restoredCount = await _unitOfWork.MedicalSupplyRepository.RestoreSuppliesAsync(deletedSupplyIds, currentUserId);

                        if (restoredCount > 0)
                        {
                            result.SuccessCount = restoredCount;
                            result.SuccessIds = deletedSupplyIds.Select(id => id.ToString()).ToList();

                            _logger.LogInformation("Restored {Count} medical supplies", restoredCount);
                        }
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch restore of medical supplies");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Business Logic Operations

        public async Task<ApiResult<List<MedicalSupplyResponseDTO>>> GetLowStockSuppliesAsync()
        {
            try
            {
                var supplies = await _unitOfWork.MedicalSupplyRepository.GetLowStockSuppliesAsync();
                var supplyDTOs = supplies.Select(MedicalSupplyMapper.MapToResponseDTO).ToList();

                return ApiResult<List<MedicalSupplyResponseDTO>>.Success(
                    supplyDTOs, "Lấy danh sách vật tư y tế sắp hết hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock supplies");
                return ApiResult<List<MedicalSupplyResponseDTO>>.Failure(ex);
            }
        }


        public async Task<ApiResult<bool>> ReconcileStockAsync(Guid id, int actualPhysicalCount)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (actualPhysicalCount < 0)
                    {
                        return ApiResult<bool>.Failure(
                            new ArgumentException("Số lượng tồn kho thực tế không được âm"));
                    }

                    var success = await _unitOfWork.MedicalSupplyRepository.ReconcileStockAsync(id, actualPhysicalCount);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException("Không tìm thấy vật tư y tế để kiểm kê"));
                    }

                    _logger.LogInformation("Reconciled stock for supply {SupplyId} to {ActualCount}", id, actualPhysicalCount);

                    return ApiResult<bool>.Success(true, "Kiểm kê và điều chỉnh tồn kho thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reconciling stock for supply: {SupplyId}", id);
                    return ApiResult<bool>.Failure(ex);
                }
            });
        }
        public async Task<ApiResult<bool>> UpdateMinimumStockAsync(Guid id, int newMinimumStock)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (newMinimumStock < 0)
                    {
                        return ApiResult<bool>.Failure(
                            new ArgumentException("Số lượng tồn kho tối thiểu không được âm"));
                    }

                    var success = await _unitOfWork.MedicalSupplyRepository.UpdateMinimumStockAsync(id, newMinimumStock);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException("Không tìm thấy vật tư y tế"));
                    }

                    _logger.LogInformation("Updated minimum stock for supply {SupplyId} to {MinStock}", id, newMinimumStock);

                    return ApiResult<bool>.Success(true, "Cập nhật tồn kho tối thiểu thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating minimum stock for supply: {SupplyId}", id);
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
            // Permanent delete - get all supplies (including deleted ones)
            var allSupplies = await _unitOfWork.MedicalSupplyRepository.GetMedicalSuppliesByIdsAsync(ids, includeDeleted: true);
            var existingIds = allSupplies.Select(s => s.Id).ToList();
            var notFoundIds = ids.Except(existingIds).ToList();

            // Add errors for not found supplies
            AddErrorsForNotFoundItems(result, notFoundIds, "Không tìm thấy vật tư y tế");

            if (existingIds.Any())
            {
                // Check supplies with active lots
                var suppliesWithActiveLots = new List<Guid>();
                foreach (var supplyId in existingIds)
                {
                    var hasActiveLots = await _unitOfWork.MedicalSupplyLotRepository.HasActiveLotsAsync(supplyId);
                    if (hasActiveLots)
                    {
                        suppliesWithActiveLots.Add(supplyId);
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = supplyId.ToString(),
                            Error = "Có lô vật tư đang hoạt động",
                            Details = $"Medical supply với ID {supplyId} có lô vật tư đang hoạt động và không thể xóa vĩnh viễn"
                        });
                    }
                }

                // Execute permanent delete for valid supplies
                var validIdsForDeletion = existingIds.Except(suppliesWithActiveLots).ToList();
                if (validIdsForDeletion.Any())
                {
                    var deletedCount = await _unitOfWork.MedicalSupplyRepository.PermanentDeleteSuppliesAsync(validIdsForDeletion);

                    if (deletedCount > 0)
                    {
                        result.SuccessCount = deletedCount;
                        result.SuccessIds = validIdsForDeletion.Select(id => id.ToString()).ToList();

                        _logger.LogInformation("Permanently deleted {Count} medical supplies", deletedCount);
                    }
                }
            }
        }

        private async Task ProcessSoftDelete(List<Guid> ids, BatchOperationResultDTO result)
        {
            foreach (var id in ids)
            {
                try
                {
                    var supply = await _unitOfWork.MedicalSupplyRepository.GetByIdAsync(id);
                    if (supply == null)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Không tìm thấy vật tư y tế",
                            Details = $"Medical supply với ID {id} không tồn tại"
                        });
                        continue;
                    }

                    if (supply.IsDeleted)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Đã bị xóa",
                            Details = "Vật tư y tế đã được xóa trước đó"
                        });
                        continue;
                    }

                    // Sử dụng BaseService DeleteAsync
                    var deleteResult = await DeleteAsync(id);
                    if (deleteResult)
                    {
                        result.SuccessIds.Add(id.ToString());
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Xóa thất bại",
                            Details = "Không thể xóa vật tư y tế"
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

        /// <summary>
        /// Validate batch input parameters
        /// </summary>
        private static (bool isValid, string message) ValidateBatchInput(List<Guid> ids, string operation)
        {
            if (ids == null || !ids.Any())
            {
                return (false, "Danh sách ID không được rỗng");
            }

            if (ids.Any(id => id == Guid.Empty))
            {
                return (false, "Danh sách chứa ID không hợp lệ");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Add errors for items not found
        /// </summary>
        private static void AddErrorsForNotFoundItems(BatchOperationResultDTO result, List<Guid> notFoundIds, string errorMessage)
        {
            foreach (var notFoundId in notFoundIds)
            {
                result.Errors.Add(new BatchOperationErrorDTO
                {
                    Id = notFoundId.ToString(),
                    Error = errorMessage,
                    Details = $"Medical supply với ID {notFoundId} {errorMessage.ToLower()}"
                });
            }
        }

        /// <summary>
        /// Generate appropriate message for batch operations
        /// </summary>
        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công {result.SuccessCount} vật tư y tế";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} vật tư y tế. " +
                       $"{result.FailureCount} thất bại";

            return $"Không thể {operation} vật tư y tế nào. Tất cả {result.FailureCount} yêu cầu đều thất bại";
        }

        /// <summary>
        /// Validate create request
        /// </summary>
        private async Task<(bool IsSuccess, string Message)> ValidateCreateRequestAsync(CreateMedicalSupplyRequest request)
        {
            var nameExists = await _unitOfWork.MedicalSupplyRepository.NameExistsAsync(request.Name);
            if (nameExists)
            {
                return (false, $"Tên vật tư y tế '{request.Name}' đã tồn tại");
            }

            if (request.MinimumStock < 0)
            {
                return (false, "Tồn kho tối thiểu không được âm");
            }

            return (true, "Valid");
        }

        /// <summary>
        /// Validate update request
        /// </summary>
        private async Task<(bool IsSuccess, string Message)> ValidateUpdateRequestAsync(UpdateMedicalSupplyRequest request, Guid excludeId)
        {
            var nameExists = await _unitOfWork.MedicalSupplyRepository.NameExistsAsync(request.Name, excludeId);
            if (nameExists)
            {
                return (false, $"Tên vật tư y tế '{request.Name}' đã tồn tại");
            }

            if (request.MinimumStock < 0)
            {
                return (false, "Tồn kho tối thiểu không được âm");
            }

            return (true, "Valid");
        }
        #endregion
    }
}