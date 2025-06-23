using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using System.Data;

namespace Services.Implementations
{
    public class VaccineLotService : BaseService<MedicationLot, Guid>, IVaccineLotService
    {
        private readonly IVaccineLotRepository _vaccineLotRepository;
        private readonly IVaccineTypeRepository _vaccineTypeRepository;
        private readonly ILogger<VaccineLotService> _logger;

        public VaccineLotService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccineLotService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccineLotRepository, currentUserService, unitOfWork, currentTime)
        {
            _vaccineLotRepository = unitOfWork.VaccineLotRepository;
            _vaccineTypeRepository = unitOfWork.VaccineTypeRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<VaccineLotResponseDTO>>> GetVaccineLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? vaccineTypeId = null, bool? isExpired = null)
        {
            try
            {
                var lots = await _vaccineLotRepository.GetVaccineLotsAsync(
                    pageNumber, pageSize, searchTerm, vaccineTypeId, isExpired);

                var result = VaccineLotMapper.MapToPagedResponseDTO(lots);

                return ApiResult<PagedList<VaccineLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô vaccine thành công");
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
                var lot = await _vaccineLotRepository.GetVaccineLotWithDetailsAsync(id);
                if (lot == null)
                {
                    return ApiResult<VaccineLotResponseDTO>.Failure(
                        new Exception("Không tìm thấy lô vaccine"));
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
                        return ApiResult<VaccineLotResponseDTO>.Failure(new Exception(message));
                    }

                    var vaccineLot = VaccineLotMapper.MapFromCreateRequest(request);
                    var createdLot = await CreateAsync(vaccineLot);

                    await _unitOfWork.SaveChangesAsync();

                    var response = VaccineLotMapper.MapToResponseDTO(createdLot);
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
                    var existingLot = await _vaccineLotRepository.GetVaccineLotByIdAsync(id);
                    if (existingLot == null)
                    {
                        return ApiResult<VaccineLotResponseDTO>.Failure(
                            new Exception("Không tìm thấy lô vaccine"));
                    }

                    var (isValid, message) = await ValidateUpdateRequestAsync(request, id);
                    if (!isValid)
                    {
                        return ApiResult<VaccineLotResponseDTO>.Failure(new Exception(message));
                    }

                    VaccineLotMapper.MapFromUpdateRequest(request, existingLot);
                    var updatedLot = await UpdateAsync(existingLot);

                    await _unitOfWork.SaveChangesAsync();

                    var response = VaccineLotMapper.MapToResponseDTO(updatedLot);
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
                    var currentUserId = _currentUserService.GetUserId();
                    if (!currentUserId.HasValue)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new UnauthorizedAccessException("Người dùng chưa đăng nhập"));
                    }

                    var result = new BatchOperationResultDTO { TotalRequested = ids.Count };

                    var deletedCount = await _vaccineLotRepository.SoftDeleteVaccineLotsAsync(ids, currentUserId.Value);
                    result.SuccessCount = deletedCount;
                    result.FailureCount = ids.Count - deletedCount;

                    if (deletedCount > 0)
                    {
                        result.SuccessIds = ids.Take(deletedCount).Select(id => id.ToString()).ToList();
                    }

                    if (result.FailureCount > 0)
                    {
                        var failedIds = ids.Skip(deletedCount);
                        foreach (var failedId in failedIds)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = failedId.ToString(),
                                Error = "Không thể xóa",
                                Details = "Lô vaccine không tồn tại hoặc đã bị xóa"
                            });
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();

                    result.Message = GenerateBatchOperationMessage("xóa", result);
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
                    var currentUserId = _currentUserService.GetUserId();
                    if (!currentUserId.HasValue)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new UnauthorizedAccessException("Người dùng chưa đăng nhập"));
                    }

                    var result = new BatchOperationResultDTO { TotalRequested = ids.Count };

                    var restoredCount = await _vaccineLotRepository.RestoreVaccineLotsAsync(ids, currentUserId.Value);
                    result.SuccessCount = restoredCount;
                    result.FailureCount = ids.Count - restoredCount;

                    if (restoredCount > 0)
                    {
                        result.SuccessIds = ids.Take(restoredCount).Select(id => id.ToString()).ToList();
                    }

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

                    await _unitOfWork.SaveChangesAsync();

                    result.Message = GenerateBatchOperationMessage("khôi phục", result);
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
                var lots = await _vaccineLotRepository.GetExpiringVaccineLotsAsync(daysBeforeExpiry);
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
                var lots = await _vaccineLotRepository.GetExpiredVaccineLotsAsync();
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
                var lots = await _vaccineLotRepository.GetLotsByVaccineTypeAsync(vaccineTypeId);
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
                        return ApiResult<bool>.Failure(new Exception("Số lượng không được âm"));
                    }

                    var success = await _vaccineLotRepository.UpdateVaccineQuantityAsync(lotId, newQuantity);
                    if (!success)
                    {
                        return ApiResult<bool>.Failure(new Exception("Không tìm thấy lô vaccine"));
                    }

                    await _unitOfWork.SaveChangesAsync();
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
                var lots = await _vaccineLotRepository.GetSoftDeletedVaccineLotsAsync(
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

                var statistics = await _vaccineLotRepository.GetVaccineLotStatisticsAsync(currentDate, expiryThreshold);

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

        private string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount} lô vaccine";
            }
            else if (result.IsCompleteFailure)
            {
                return $"Không thể {operation} bất kỳ lô vaccine nào. {result.FailureCount} lỗi";
            }
            else if (result.IsPartialSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} lô vaccine. {result.FailureCount} lỗi";
            }
            else
            {
                return $"Không có lô vaccine nào được {operation}";
            }
        }

        private async Task<(bool IsSuccess, string Message)> ValidateCreateRequestAsync(CreateVaccineLotRequest request)
        {
            var vaccineType = await _vaccineTypeRepository.GetByIdAsync(request.VaccineTypeId);
            if (vaccineType == null)
            {
                return (false, "Không tìm thấy loại vaccine");
            }

            var lotNumberExists = await _vaccineLotRepository.VaccineLotNumberExistsAsync(request.LotNumber);
            if (lotNumberExists)
            {
                return (false, $"Số lô vaccine '{request.LotNumber}' đã tồn tại");
            }

            if (request.ExpiryDate.Date <= _currentTime.GetVietnamTime().Date)
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
            var lotNumberExists = await _vaccineLotRepository.VaccineLotNumberExistsAsync(request.LotNumber, excludeId);
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