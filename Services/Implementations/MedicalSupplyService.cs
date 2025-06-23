using Microsoft.Extensions.Logging;
using System.Data;

namespace Services.Implementations
{
    public class MedicalSupplyService : BaseService<MedicalSupply, Guid>, IMedicalSupplyService
    {
        private readonly IMedicalSupplyRepository _medicalSupplyRepository;
        private readonly IMedicalSupplyLotRepository _medicalSupplyLotRepository;
        private readonly ILogger<MedicalSupplyService> _logger;

        public MedicalSupplyService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<MedicalSupplyService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.MedicalSupplyRepository, currentUserService, unitOfWork, currentTime)
        {
            _medicalSupplyRepository = unitOfWork.MedicalSupplyRepository;
            _medicalSupplyLotRepository = unitOfWork.MedicalSupplyLotRepository;
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
                var supplies = await _medicalSupplyRepository.GetMedicalSuppliesAsync(
                    pageNumber,
                    pageSize,
                    searchTerm,
                    isActive,
                    includeDeleted);

                var dtoList = supplies.Select(MapToResponseDTO).ToList();
                var resultPage = new PagedList<MedicalSupplyResponseDTO>(
                    dtoList,
                    supplies.MetaData.TotalCount,
                    supplies.MetaData.CurrentPage,
                    supplies.MetaData.PageSize);

                return ApiResult<PagedList<MedicalSupplyResponseDTO>>.Success(
                    resultPage, "Lấy danh sách vật tư y tế thành công");
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
                var supply = await _medicalSupplyRepository.GetByIdAsync(id);
                if (supply == null)
                {
                    return ApiResult<MedicalSupplyResponseDTO>.Failure(
                        new Exception("Không tìm thấy vật tư y tế"));
                }

                var supplyDTO = MapToResponseDTO(supply);
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
                var supply = await _medicalSupplyRepository.GetSupplyWithLotsAsync(id);
                if (supply == null)
                {
                    return ApiResult<MedicalSupplyDetailResponseDTO>.Failure(
                        new Exception("Không tìm thấy vật tư y tế"));
                }

                var supplyDetailDTO = MapToDetailResponseDTO(supply);
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
                            new Exception(validationResult.Message));
                    }

                    var supply = MapFromCreateRequest(request);
                    var createdSupply = await CreateAsync(supply);

                    var supplyDTO = MapToResponseDTO(createdSupply);
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
                    var supply = await _medicalSupplyRepository.GetByIdAsync(id);
                    if (supply == null)
                    {
                        return ApiResult<MedicalSupplyResponseDTO>.Failure(
                            new Exception("Không tìm thấy vật tư y tế"));
                    }

                    var validationResult = await ValidateUpdateRequestAsync(request, id);
                    if (!validationResult.IsSuccess)
                    {
                        return ApiResult<MedicalSupplyResponseDTO>.Failure(
                            new Exception(validationResult.Message));
                    }

                    MapFromUpdateRequest(request, supply);
                    await UpdateAsync(supply);

                    var supplyDTO = MapToResponseDTO(supply);
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
                var supplies = await _medicalSupplyRepository.GetSoftDeletedSuppliesAsync(
                    pageNumber, pageSize, searchTerm);

                var supplyDTOs = supplies.Select(MapToResponseDTO).ToList();
                var result = CreatePagedResult(supplies, supplyDTOs);

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

        /// <summary>
        /// Xóa vật tư y tế (hỗ trợ cả xóa mềm và xóa vĩnh viễn)
        /// </summary>
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
                        // Permanent delete - get all supplies (including deleted ones)
                        var allSupplies = await _medicalSupplyRepository.GetMedicalSuppliesByIdsAsync(ids, includeDeleted: true);
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
                                var hasActiveLots = await _medicalSupplyLotRepository.HasActiveLotsAsync(supplyId);
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
                                var deletedCount = await _medicalSupplyRepository.PermanentDeleteSuppliesAsync(validIdsForDeletion);

                                if (deletedCount > 0)
                                {
                                    result.SuccessCount = deletedCount;
                                    result.SuccessIds = validIdsForDeletion.Select(id => id.ToString()).ToList();
                                }
                            }
                        }
                    }
                    else
                    {
                        // Soft delete - only get non-deleted supplies
                        var existingSupplies = await _medicalSupplyRepository.GetMedicalSuppliesByIdsAsync(ids, includeDeleted: false);
                        var existingIds = existingSupplies.Select(s => s.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        // Add errors for not found or already deleted supplies
                        AddErrorsForNotFoundItems(result, notFoundIds, "Không tìm thấy vật tư y tế hoặc đã bị xóa");

                        // Execute soft delete for valid supplies
                        if (existingIds.Any())
                        {
                            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                            var deletedCount = await _medicalSupplyRepository.SoftDeleteSuppliesAsync(existingIds, currentUserId);

                            if (deletedCount > 0)
                            {
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
                    _logger.LogError(ex, "Error during batch delete of medical supplies (isPermanent: {IsPermanent})", isPermanent);
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Khôi phục vật tư y tế (hỗ trợ cả đơn lẻ và hàng loạt)
        /// </summary>
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
                    var deletedSupplies = await _medicalSupplyRepository.GetMedicalSuppliesByIdsAsync(ids, includeDeleted: true);
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
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var restoredCount = await _medicalSupplyRepository.RestoreSuppliesAsync(deletedSupplyIds, currentUserId);

                        if (restoredCount > 0)
                        {
                            result.SuccessCount = restoredCount;
                            result.SuccessIds = deletedSupplyIds.Select(id => id.ToString()).ToList();
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
                var supplies = await _medicalSupplyRepository.GetLowStockSuppliesAsync();
                var supplyDTOs = supplies.Select(MapToResponseDTO).ToList();

                return ApiResult<List<MedicalSupplyResponseDTO>>.Success(
                    supplyDTOs, "Lấy danh sách vật tư y tế sắp hết hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock supplies");
                return ApiResult<List<MedicalSupplyResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> UpdateCurrentStockAsync(Guid id, int newStock)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (newStock < 0)
                    {
                        return ApiResult<bool>.Failure(
                            new Exception("Số lượng tồn kho không được âm"));
                    }

                    var success = await _medicalSupplyRepository.UpdateCurrentStockAsync(id, newStock);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new Exception("Không tìm thấy vật tư y tế"));
                    }

                    return ApiResult<bool>.Success(true, "Cập nhật tồn kho thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating current stock for supply: {SupplyId}", id);
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
                            new Exception("Số lượng tồn kho tối thiểu không được âm"));
                    }

                    var success = await _medicalSupplyRepository.UpdateMinimumStockAsync(id, newMinimumStock);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new Exception("Không tìm thấy vật tư y tế"));
                    }

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
            {
                return $"Đã {operation} thành công {result.SuccessCount} vật tư y tế";
            }
            else if (result.IsCompleteFailure)
            {
                return $"Không thể {operation} bất kỳ vật tư y tế nào. {result.FailureCount} lỗi";
            }
            else if (result.IsPartialSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} vật tư y tế. {result.FailureCount} lỗi";
            }
            else
            {
                return $"Không có vật tư y tế nào được {operation}";
            }
        }

        /// <summary>
        /// Validate create request
        /// </summary>
        private async Task<(bool IsSuccess, string Message)> ValidateCreateRequestAsync(CreateMedicalSupplyRequest request)
        {
            var nameExists = await _medicalSupplyRepository.NameExistsAsync(request.Name);
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
            var nameExists = await _medicalSupplyRepository.NameExistsAsync(request.Name, excludeId);
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
        /// Map from create request to entity
        /// </summary>
        private static MedicalSupply MapFromCreateRequest(CreateMedicalSupplyRequest request)
        {
            return new MedicalSupply
            {
                Name = request.Name,
                Unit = request.Unit,
                CurrentStock = 0, // Mặc định bằng 0 khi tạo mới
                MinimumStock = request.MinimumStock,
                IsActive = request.IsActive
            };
        }

        /// <summary>
        /// Map from update request to entity
        /// </summary>
        private static void MapFromUpdateRequest(UpdateMedicalSupplyRequest request, MedicalSupply supply)
        {
            supply.Name = request.Name;
            supply.Unit = request.Unit;
            supply.MinimumStock = request.MinimumStock;
            supply.IsActive = request.IsActive;
        }

        /// <summary>
        /// Map entity to response DTO
        /// </summary>
        private static MedicalSupplyResponseDTO MapToResponseDTO(MedicalSupply supply)
        {
            return new MedicalSupplyResponseDTO
            {
                Id = supply.Id,
                Name = supply.Name,
                Unit = supply.Unit,
                CurrentStock = supply.CurrentStock,
                MinimumStock = supply.MinimumStock,
                IsActive = supply.IsActive,
                IsDeleted = supply.IsDeleted,
                CreatedAt = supply.CreatedAt,
                UpdatedAt = supply.UpdatedAt,
                CreatedBy = supply.CreatedBy.ToString() ?? string.Empty,
                UpdatedBy = supply.UpdatedBy.ToString() ?? string.Empty
            };
        }

        /// <summary>
        /// Map entity to detail response DTO
        /// </summary>
        private static MedicalSupplyDetailResponseDTO MapToDetailResponseDTO(MedicalSupply supply)
        {
            return new MedicalSupplyDetailResponseDTO
            {
                Id = supply.Id,
                Name = supply.Name,
                Unit = supply.Unit,
                CurrentStock = supply.CurrentStock,
                MinimumStock = supply.MinimumStock,
                IsActive = supply.IsActive,
                IsDeleted = supply.IsDeleted,
                CreatedAt = supply.CreatedAt,
                UpdatedAt = supply.UpdatedAt,
                CreatedBy = supply.CreatedBy.ToString() ?? string.Empty,
                UpdatedBy = supply.UpdatedBy.ToString() ?? string.Empty,
                TotalLots = supply.Lots?.Count ?? 0,
                Lots = supply.Lots?.Select(lot => new MedicalSupplyLotDetailResponseDTO
                {
                    Id = lot.Id,
                    LotNumber = lot.LotNumber,
                    ExpirationDate = lot.ExpirationDate,
                    ManufactureDate = lot.ManufactureDate,
                    Quantity = lot.Quantity,
                    CreatedAt = lot.CreatedAt,
                    UpdatedAt = lot.UpdatedAt
                }).OrderBy(l => l.ExpirationDate).ToList() ?? new List<MedicalSupplyLotDetailResponseDTO>()
            };
        }

        /// <summary>
        /// Create paged result from source
        /// </summary>
        private static PagedList<MedicalSupplyResponseDTO> CreatePagedResult(
            PagedList<MedicalSupply> sourcePaged,
            List<MedicalSupplyResponseDTO> mappedItems)
        {
            return new PagedList<MedicalSupplyResponseDTO>(
                mappedItems,
                sourcePaged.MetaData.TotalCount,
                sourcePaged.MetaData.CurrentPage,
                sourcePaged.MetaData.PageSize);
        }

        #endregion
    }
}