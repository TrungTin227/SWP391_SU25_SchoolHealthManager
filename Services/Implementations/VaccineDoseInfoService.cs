using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccineDoseInfoService : BaseService<VaccineDoseInfo, Guid>, IVaccineDoseInfoService
    {
        private readonly ILogger<VaccineDoseInfoService> _logger;

        public VaccineDoseInfoService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccineDoseInfoService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccineDoseInfoRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<VaccineDoseInfoResponseDTO>>> GetVaccineDoseInfosAsync(
            int pageNumber, int pageSize, Guid? vaccineTypeId = null, int? doseNumber = null)
        {
            try
            {
                var pagedDoseInfos = await _unitOfWork.VaccineDoseInfoRepository.GetVaccineDoseInfosAsync(
                    pageNumber, pageSize, vaccineTypeId, doseNumber);

                var responseItems = pagedDoseInfos.Select(VaccineDoseInfoMapper.MapToResponseDTO).ToList();
                var pagedResult = VaccineDoseInfoMapper.ToPagedResult(pagedDoseInfos, responseItems);

                return ApiResult<PagedList<VaccineDoseInfoResponseDTO>>.Success(
                    pagedResult,
                    $"Lấy danh sách thông tin liều vaccine thành công. Tổng số: {pagedResult.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông tin liều vaccine");
                return ApiResult<PagedList<VaccineDoseInfoResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineDoseInfoResponseDTO>> GetVaccineDoseInfoByIdAsync(Guid id)
        {
            try
            {
                var doseInfo = await _unitOfWork.VaccineDoseInfoRepository.GetVaccineDoseInfoByIdAsync(id);
                if (doseInfo == null)
                {
                    return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy thông tin liều vaccine với ID: {id}"));
                }

                var response = VaccineDoseInfoMapper.MapToResponseDTO(doseInfo);
                return ApiResult<VaccineDoseInfoResponseDTO>.Success(response, "Lấy thông tin liều vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin liều vaccine với ID: {Id}", id);
                return ApiResult<VaccineDoseInfoResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineDoseInfoDetailResponseDTO>> GetVaccineDoseInfoDetailByIdAsync(Guid id)
        {
            try
            {
                var doseInfo = await _unitOfWork.VaccineDoseInfoRepository.GetVaccineDoseInfoWithDetailsAsync(id);
                if (doseInfo == null)
                {
                    return ApiResult<VaccineDoseInfoDetailResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy thông tin liều vaccine với ID: {id}"));
                }

                var response = VaccineDoseInfoMapper.MapToDetailResponseDTO(doseInfo);
                return ApiResult<VaccineDoseInfoDetailResponseDTO>.Success(response, "Lấy chi tiết thông tin liều vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết thông tin liều vaccine với ID: {Id}", id);
                return ApiResult<VaccineDoseInfoDetailResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineDoseInfoResponseDTO>> CreateVaccineDoseInfoAsync(CreateVaccineDoseInfoRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate vaccine type exists
                    var vaccineType = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(request.VaccineTypeId);
                    if (vaccineType == null)
                    {
                        return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                            new KeyNotFoundException($"Không tìm thấy loại vaccine với ID: {request.VaccineTypeId}"));
                    }

                    // Validate unique dose number for vaccine type
                    var isDoseNumberExists = await _unitOfWork.VaccineDoseInfoRepository.IsDoseNumberExistsAsync(
                        request.VaccineTypeId, request.DoseNumber);
                    if (isDoseNumberExists)
                    {
                        return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                            new ArgumentException($"Mũi tiêm số {request.DoseNumber} đã tồn tại cho vaccine {vaccineType.Name}"));
                    }

                    // Validate previous dose if specified
                    if (request.PreviousDoseId.HasValue)
                    {
                        var validationResult = await ValidatePreviousDoseAsync(request.PreviousDoseId.Value, request.VaccineTypeId, request.DoseNumber);
                        if (!validationResult.IsValid)
                        {
                            return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                                new ArgumentException(validationResult.Message));
                        }
                    }

                    var doseInfo = VaccineDoseInfoMapper.MapFromCreateRequest(request);

                    // BaseService sẽ tự động xử lý audit fields
                    var createdDoseInfo = await CreateAsync(doseInfo);
                    var response = VaccineDoseInfoMapper.MapToResponseDTO(createdDoseInfo);

                    _logger.LogInformation("Tạo thông tin liều vaccine thành công: {VaccineType} - Mũi {DoseNumber}",
                        vaccineType.Name, request.DoseNumber);

                    return ApiResult<VaccineDoseInfoResponseDTO>.Success(response, "Tạo thông tin liều vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo thông tin liều vaccine");
                    return ApiResult<VaccineDoseInfoResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccineDoseInfoResponseDTO>> UpdateVaccineDoseInfoAsync(Guid id, UpdateVaccineDoseInfoRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var existingDoseInfo = await _unitOfWork.VaccineDoseInfoRepository.GetVaccineDoseInfoByIdAsync(id);
                    if (existingDoseInfo == null)
                    {
                        return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                            new KeyNotFoundException($"Không tìm thấy thông tin liều vaccine với ID: {id}"));
                    }

                    // Validate dose number if changed
                    if (request.DoseNumber.HasValue && request.DoseNumber.Value != existingDoseInfo.DoseNumber)
                    {
                        var isDoseNumberExists = await _unitOfWork.VaccineDoseInfoRepository.IsDoseNumberExistsAsync(
                            existingDoseInfo.VaccineTypeId, request.DoseNumber.Value, id);
                        if (isDoseNumberExists)
                        {
                            return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                                new ArgumentException($"Mũi tiêm số {request.DoseNumber.Value} đã tồn tại"));
                        }
                    }

                    VaccineDoseInfoMapper.UpdateFromRequest(existingDoseInfo, request);

                    // BaseService sẽ tự động xử lý audit fields
                    var updatedDoseInfo = await UpdateAsync(existingDoseInfo);
                    var response = VaccineDoseInfoMapper.MapToResponseDTO(updatedDoseInfo);

                    _logger.LogInformation("Cập nhật thông tin liều vaccine thành công: {Id}", id);
                    return ApiResult<VaccineDoseInfoResponseDTO>.Success(response, "Cập nhật thông tin liều vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật thông tin liều vaccine với ID: {Id}", id);
                    return ApiResult<VaccineDoseInfoResponseDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineDoseInfosAsync(List<Guid> ids, bool isPermanent = false)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchInput(ids, isPermanent ? "xóa vĩnh viễn" : "xóa");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    var operationType = isPermanent ? "xóa vĩnh viễn" : "xóa";
                    _logger.LogInformation("Starting batch {Operation} for {Count} vaccine dose infos",
                        operationType, ids.Count);

                    var result = new BatchOperationResultDTO { TotalRequested = ids.Count };

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

                    _logger.LogInformation("Batch {Operation} completed: {SuccessCount}/{TotalCount} vaccine dose infos",
                        operationType, result.SuccessCount, result.TotalRequested);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thực hiện batch delete thông tin liều vaccine");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Business Operations

        public async Task<ApiResult<List<VaccineDoseInfoResponseDTO>>> GetDoseInfosByVaccineTypeAsync(Guid vaccineTypeId)
        {
            try
            {
                var doseInfos = await _unitOfWork.VaccineDoseInfoRepository.GetDoseInfosByVaccineTypeAsync(vaccineTypeId);
                var response = doseInfos.Select(VaccineDoseInfoMapper.MapToResponseDTO).ToList();

                return ApiResult<List<VaccineDoseInfoResponseDTO>>.Success(
                    response, $"Lấy thông tin liều vaccine theo loại thành công. Tổng số: {response.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin liều vaccine theo loại với ID: {VaccineTypeId}", vaccineTypeId);
                return ApiResult<List<VaccineDoseInfoResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineDoseInfoResponseDTO>> GetNextRecommendedDoseAsync(Guid vaccineTypeId, int currentDoseNumber)
        {
            try
            {
                var nextDoseNumber = currentDoseNumber + 1;
                var nextDose = await _unitOfWork.VaccineDoseInfoRepository.GetDoseInfoByVaccineTypeAndDoseNumberAsync(
                    vaccineTypeId, nextDoseNumber);

                if (nextDose == null)
                {
                    return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                        new KeyNotFoundException($"Không có mũi tiêm thứ {nextDoseNumber} cho loại vaccine này"));
                }

                var response = VaccineDoseInfoMapper.MapToResponseDTO(nextDose);
                return ApiResult<VaccineDoseInfoResponseDTO>.Success(response, "Lấy thông tin mũi tiêm tiếp theo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy mũi tiêm tiếp theo cho vaccine {VaccineTypeId}, mũi hiện tại {CurrentDose}",
                    vaccineTypeId, currentDoseNumber);
                return ApiResult<VaccineDoseInfoResponseDTO>.Failure(ex);
            }
        }

        #endregion

        #region Private Helper Methods

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
                return (false, $"Không thể {operation} quá 100 thông tin liều vaccine cùng lúc");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate previous dose
        /// </summary>
        private async Task<(bool IsValid, string Message)> ValidatePreviousDoseAsync(Guid previousDoseId, Guid vaccineTypeId, int doseNumber)
        {
            var previousDose = await _unitOfWork.VaccineDoseInfoRepository.GetByIdAsync(previousDoseId);
            if (previousDose == null || previousDose.VaccineTypeId != vaccineTypeId)
            {
                return (false, "Mũi tiêm trước không hợp lệ");
            }

            if (previousDose.DoseNumber >= doseNumber)
            {
                return (false, "Số mũi tiêm phải lớn hơn mũi tiêm trước");
            }

            return (true, "Valid");
        }

        /// <summary>
        /// Process permanent delete operations
        /// </summary>
        private async Task ProcessPermanentDelete(List<Guid> ids, BatchOperationResultDTO result)
        {
            var doseInfos = await _unitOfWork.VaccineDoseInfoRepository.GetVaccineDoseInfosByIdsAsync(ids, includeDeleted: true);
            var foundIds = doseInfos.Select(d => d.Id).ToHashSet();

            foreach (var id in ids)
            {
                try
                {
                    if (!foundIds.Contains(id))
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Không tìm thấy",
                            Details = $"Thông tin liều vaccine với ID {id} không tồn tại"
                        });
                        continue;
                    }

                    var doseInfo = doseInfos.First(d => d.Id == id);

                    // Check business rules before permanent delete
                    var businessValidation = await ValidateBeforeDeleteAsync(id, doseInfo.VaccineTypeId);
                    if (!businessValidation.IsValid)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Không thể xóa",
                            Details = businessValidation.Message
                        });
                        continue;
                    }

                    await _unitOfWork.VaccineDoseInfoRepository.DeleteAsync(id);
                    result.SuccessIds.Add(id.ToString());
                    result.SuccessCount++;
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
        /// Process soft delete operations
        /// </summary>
        private async Task ProcessSoftDelete(List<Guid> ids, BatchOperationResultDTO result)
        {
            foreach (var id in ids)
            {
                try
                {
                    var doseInfo = await _unitOfWork.VaccineDoseInfoRepository.GetVaccineDoseInfoByIdAsync(id);
                    if (doseInfo == null)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Không tìm thấy",
                            Details = $"Thông tin liều vaccine với ID {id} không tồn tại"
                        });
                        continue;
                    }

                    if (doseInfo.IsDeleted)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Đã bị xóa",
                            Details = "Thông tin liều vaccine đã được xóa trước đó"
                        });
                        continue;
                    }

                    // Check business rules before soft delete
                    var businessValidation = await ValidateBeforeDeleteAsync(id, doseInfo.VaccineTypeId);
                    if (!businessValidation.IsValid)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Không thể xóa",
                            Details = businessValidation.Message
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
                            Details = "Không thể xóa thông tin liều vaccine"
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
        /// Validate business rules before delete
        /// </summary>
        private async Task<(bool IsValid, string Message)> ValidateBeforeDeleteAsync(Guid doseInfoId, Guid vaccineTypeId)
        {
            // Check if this is the only dose for the vaccine type
            var totalDoses = await _unitOfWork.VaccineDoseInfoRepository.GetDoseInfosByVaccineTypeAsync(vaccineTypeId);
            if (totalDoses.Count == 1 && totalDoses.First().Id == doseInfoId)
            {
                return (false, "Không thể xóa mũi tiêm duy nhất của loại vaccine");
            }

            // Check if other doses depend on this dose
            var nextDoses = await _unitOfWork.VaccineDoseInfoRepository.GetNextDosesAsync(doseInfoId);
            if (nextDoses.Any())
            {
                return (false, $"Có {nextDoses.Count} mũi tiêm kế tiếp phụ thuộc vào mũi tiêm này");
            }

            return (true, "Valid");
        }

        /// <summary>
        /// Generate batch operation message
        /// </summary>
        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công {result.SuccessCount} thông tin liều vaccine";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} thông tin liều vaccine. " +
                       $"{result.FailureCount} thất bại";

            return $"Không thể {operation} thông tin liều vaccine nào. Tất cả {result.FailureCount} yêu cầu đều thất bại";
        }

        #endregion
    }
}