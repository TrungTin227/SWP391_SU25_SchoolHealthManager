using Microsoft.Extensions.Logging;
using System.Data;

namespace Services.Implementations
{
    public class VaccineLotService : BaseService<MedicationLot, Guid>, IVaccineLotService
    {
        private readonly ILogger<VaccineLotService> _logger;

        public VaccineLotService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccineLotService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccineLotRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<VaccineLotResponseDTO>>> GetVaccineLotsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            Guid? vaccineTypeId = null,
            bool? isExpired = null,
            int? daysBeforeExpiry = null,
            bool? isDeleted = null)
        {
            try
            {
                var lots = await _unitOfWork.VaccineLotRepository.GetVaccineLotsAsync(
                    pageNumber,
                    pageSize,
                    searchTerm,
                    vaccineTypeId,
                    isExpired,
                    daysBeforeExpiry,
                    isDeleted);

                var resultDto = VaccineLotMapper.MapToPagedResponseDTO(lots);
                return ApiResult<PagedList<VaccineLotResponseDTO>>.Success(
                    resultDto, "Lấy danh sách lô vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vaccine lots");
                return ApiResult<PagedList<VaccineLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineLotResponseDTO>> GetVaccineLotByIdAsync(Guid id)
        {
            try
            {
                var lot = await _unitOfWork.VaccineLotRepository.GetVaccineLotWithDetailsAsync(id);
                if (lot == null)
                {
                    return ApiResult<VaccineLotResponseDTO>.Failure(
                        new KeyNotFoundException("Không tìm thấy lô vaccine"));
                }

                var lotDTO = VaccineLotMapper.MapToResponseDTO(lot);
                return ApiResult<VaccineLotResponseDTO>.Success(
                    lotDTO, "Lấy thông tin lô vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vaccine lot by ID: {LotId}", id);
                return ApiResult<VaccineLotResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineLotResponseDTO>> CreateVaccineLotAsync(CreateVaccineLotRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var (isValid, message) = await ValidateCreateRequestAsync(request);
                    if (!isValid)
                    {
                        return ApiResult<VaccineLotResponseDTO>.Failure(
                            new ArgumentException(message));
                    }

                    var vaccineLot = VaccineLotMapper.MapFromCreateRequest(request);

                    // BaseService sẽ tự động xử lý audit fields
                    var createdLot = await CreateAsync(vaccineLot);

                    var response = VaccineLotMapper.MapToResponseDTO(createdLot);

                    _logger.LogInformation("Created vaccine lot: {LotId} with lot number: {LotNumber}",
                        createdLot.Id, createdLot.LotNumber);

                    return ApiResult<VaccineLotResponseDTO>.Success(response, "Tạo lô vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating vaccine lot");
                    return ApiResult<VaccineLotResponseDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<VaccineLotResponseDTO>> UpdateVaccineLotAsync(Guid id, UpdateVaccineLotRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var existingLot = await _unitOfWork.VaccineLotRepository.GetVaccineLotByIdAsync(id);
                    if (existingLot == null)
                    {
                        return ApiResult<VaccineLotResponseDTO>.Failure(
                            new KeyNotFoundException("Không tìm thấy lô vaccine"));
                    }

                    var (isValid, message) = await ValidateUpdateRequestAsync(request, id);
                    if (!isValid)
                    {
                        return ApiResult<VaccineLotResponseDTO>.Failure(
                            new ArgumentException(message));
                    }

                    VaccineLotMapper.MapFromUpdateRequest(request, existingLot);

                    // BaseService sẽ tự động xử lý audit fields
                    var updatedLot = await UpdateAsync(existingLot);

                    var response = VaccineLotMapper.MapToResponseDTO(updatedLot);

                    _logger.LogInformation("Updated vaccine lot: {LotId}", id);

                    return ApiResult<VaccineLotResponseDTO>.Success(response, "Cập nhật lô vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating vaccine lot: {LotId}", id);
                    return ApiResult<VaccineLotResponseDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineLotsAsync(List<Guid> ids)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchInput(ids, "xóa");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    _logger.LogInformation("Starting batch soft delete for {Count} vaccine lots", ids.Count);

                    var result = new BatchOperationResultDTO { TotalRequested = ids.Count };

                    foreach (var id in ids)
                    {
                        try
                        {
                            var lot = await _unitOfWork.VaccineLotRepository.GetVaccineLotByIdAsync(id);
                            if (lot == null)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Không tìm thấy lô vaccine",
                                    Details = $"Vaccine lot với ID {id} không tồn tại"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            if (lot.IsDeleted)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Đã bị xóa",
                                    Details = "Lô vaccine đã được xóa trước đó"
                                });
                                result.FailureCount++;
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
                                    Details = "Không thể xóa lô vaccine"
                                });
                                result.FailureCount++;
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
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("xóa", result);

                    _logger.LogInformation("Batch soft delete completed: {SuccessCount}/{TotalCount} vaccine lots",
                        result.SuccessCount, ids.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch delete of vaccine lots");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreVaccineLotsAsync(List<Guid> ids)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchInput(ids, "khôi phục");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    _logger.LogInformation("Starting batch restore for {Count} vaccine lots", ids.Count);

                    var currentUserId = GetCurrentUserIdOrThrow();
                    var restoredCount = await _unitOfWork.VaccineLotRepository.RestoreVaccineLotsAsync(ids, currentUserId);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count,
                        SuccessCount = restoredCount,
                        FailureCount = ids.Count - restoredCount,
                        SuccessIds = ids.Take(restoredCount).Select(id => id.ToString()).ToList()
                    };

                    if (result.FailureCount > 0)
                    {
                        var failedIds = ids.Skip(restoredCount);
                        foreach (var failedId in failedIds)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = failedId.ToString(),
                                Error = "Không thể khôi phục",
                                Details = "Lô vaccine không tồn tại hoặc chưa bị xóa"
                            });
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

                    _logger.LogInformation("Batch restore completed: {SuccessCount}/{TotalCount} vaccine lots",
                        restoredCount, ids.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch restore of vaccine lots");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Business Logic Operations

        public async Task<ApiResult<List<VaccineLotResponseDTO>>> GetExpiringVaccineLotsAsync(int daysBeforeExpiry = 30)
        {
            try
            {
                var lots = await _unitOfWork.VaccineLotRepository.GetExpiringVaccineLotsAsync(daysBeforeExpiry);
                var lotDTOs = VaccineLotMapper.MapToResponseDTOList(lots);

                return ApiResult<List<VaccineLotResponseDTO>>.Success(
                    lotDTOs, $"Lấy danh sách lô vaccine sắp hết hạn trong {daysBeforeExpiry} ngày thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring vaccine lots");
                return ApiResult<List<VaccineLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<VaccineLotResponseDTO>>> GetExpiredVaccineLotsAsync()
        {
            try
            {
                var lots = await _unitOfWork.VaccineLotRepository.GetExpiredVaccineLotsAsync();
                var lotDTOs = VaccineLotMapper.MapToResponseDTOList(lots);

                return ApiResult<List<VaccineLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô vaccine đã hết hạn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired vaccine lots");
                return ApiResult<List<VaccineLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<VaccineLotResponseDTO>>> GetLotsByVaccineTypeAsync(Guid vaccineTypeId)
        {
            try
            {
                var lots = await _unitOfWork.VaccineLotRepository.GetLotsByVaccineTypeAsync(vaccineTypeId);
                var lotDTOs = VaccineLotMapper.MapToResponseDTOList(lots);

                return ApiResult<List<VaccineLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô vaccine theo loại vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lots by vaccine type: {VaccineTypeId}", vaccineTypeId);
                return ApiResult<List<VaccineLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> UpdateVaccineQuantityAsync(Guid lotId, int newQuantity)
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

                    var success = await _unitOfWork.VaccineLotRepository.UpdateVaccineQuantityAsync(lotId, newQuantity);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException("Không tìm thấy lô vaccine"));
                    }

                    _logger.LogInformation("Updated vaccine quantity for lot {LotId} to {Quantity}", lotId, newQuantity);

                    return ApiResult<bool>.Success(true, "Cập nhật số lượng vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating vaccine quantity for lot: {LotId}", lotId);
                    return ApiResult<bool>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<VaccineLotResponseDTO>>> GetSoftDeletedVaccineLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var lots = await _unitOfWork.VaccineLotRepository.GetSoftDeletedVaccineLotsAsync(
                    pageNumber, pageSize, searchTerm);

                var result = VaccineLotMapper.MapToPagedResponseDTO(lots);

                return ApiResult<PagedList<VaccineLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô vaccine đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted vaccine lots");
                return ApiResult<PagedList<VaccineLotResponseDTO>>.Failure(ex);
            }
        }

        #endregion

        #region Statistics

        public async Task<ApiResult<VaccineLotStatisticsResponseDTO>> GetVaccineLotStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to calculate vaccine lot statistics");

                var currentDate = _currentTime.GetVietnamTime().Date;
                var expiryThreshold = currentDate.AddDays(30);

                var statistics = await _unitOfWork.VaccineLotRepository.GetVaccineLotStatisticsAsync(currentDate, expiryThreshold);

                _logger.LogInformation(
                    "Successfully calculated vaccine lot statistics: Total={TotalLots}, Active={ActiveLots}",
                    statistics.TotalLots, statistics.ActiveLots);

                return ApiResult<VaccineLotStatisticsResponseDTO>.Success(
                    statistics, "Lấy thống kê lô vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating vaccine lot statistics");
                return ApiResult<VaccineLotStatisticsResponseDTO>.Failure(ex);
            }
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

        /// <summary>
        /// Validate batch input
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

            if (ids.Count > 100)
            {
                return (false, $"Không thể {operation} quá 100 lô vaccine cùng lúc");
            }

            return (true, string.Empty);
        }

        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công {result.SuccessCount} lô vaccine";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} lô vaccine. " +
                       $"{result.FailureCount} thất bại";

            return $"Không thể {operation} lô vaccine nào. Tất cả {result.FailureCount} yêu cầu đều thất bại";
        }

        private async Task<(bool IsSuccess, string Message)> ValidateCreateRequestAsync(CreateVaccineLotRequest request)
        {
            var vaccineType = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(request.VaccineTypeId);
            if (vaccineType == null)
            {
                return (false, "Không tìm thấy loại vaccine");
            }

            var lotNumberExists = await _unitOfWork.VaccineLotRepository.VaccineLotNumberExistsAsync(request.LotNumber);
            if (lotNumberExists)
            {
                return (false, $"Số lô vaccine '{request.LotNumber}' đã tồn tại");
            }

            var currentDate = _currentTime.GetVietnamTime().Date;

            if (request.ExpiryDate.Date <= currentDate)
            {
                return (false, "Ngày hết hạn phải lớn hơn ngày hiện tại");
            }

            if (request.Quantity <= 0)
            {
                return (false, "Số lượng phải lớn hơn 0");
            }

            return (true, "Valid");
        }

        private async Task<(bool IsSuccess, string Message)> ValidateUpdateRequestAsync(UpdateVaccineLotRequest request, Guid excludeId)
        {
            var lotNumberExists = await _unitOfWork.VaccineLotRepository.VaccineLotNumberExistsAsync(request.LotNumber, excludeId);
            if (lotNumberExists)
            {
                return (false, $"Số lô vaccine '{request.LotNumber}' đã tồn tại");
            }

            if (request.Quantity < 0)
            {
                return (false, "Số lượng không được âm");
            }

            return (true, "Valid");
        }

        #endregion
    }
}