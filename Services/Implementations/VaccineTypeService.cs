using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccineTypeService : BaseService<VaccinationType, Guid>, IVaccineTypeService
    {
        private readonly ILogger<VaccineTypeService> _logger;

        public VaccineTypeService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccineTypeService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccineTypeRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<VaccineTypeResponseDTO>>> GetVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            try
            {
                var pagedVaccineTypes = await _unitOfWork.VaccineTypeRepository.GetVaccineTypesAsync(
                    pageNumber, pageSize, searchTerm, isActive);

                var responseItems = pagedVaccineTypes.Select(VaccineTypeMapper.MapToResponseDTO).ToList();
                var pagedResult = VaccineTypeMapper.ToPagedResult(pagedVaccineTypes, responseItems);

                return ApiResult<PagedList<VaccineTypeResponseDTO>>.Success(
                    pagedResult,
                    $"Lấy danh sách vaccine thành công. Tổng số: {pagedResult.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách vaccine");
                return ApiResult<PagedList<VaccineTypeResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineTypeResponseDTO>> GetVaccineTypeByIdAsync(Guid id)
        {
            try
            {
                var vaccineType = await _unitOfWork.VaccineTypeRepository.GetVaccineTypeByIdAsync(id);
                if (vaccineType == null)
                {
                    return ApiResult<VaccineTypeResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy vaccine với ID: {id}"));
                }

                var response = VaccineTypeMapper.MapToResponseDTO(vaccineType);
                return ApiResult<VaccineTypeResponseDTO>.Success(response, "Lấy thông tin vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin vaccine với ID: {Id}", id);
                return ApiResult<VaccineTypeResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineTypeDetailResponseDTO>> GetVaccineTypeDetailByIdAsync(Guid id)
        {
            try
            {
                var vaccineType = await _unitOfWork.VaccineTypeRepository.GetVaccineTypeWithDetailsAsync(id);
                if (vaccineType == null)
                {
                    return ApiResult<VaccineTypeDetailResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy vaccine với ID: {id}"));
                }

                var response = VaccineTypeMapper.MapToDetailResponseDTO(vaccineType);
                return ApiResult<VaccineTypeDetailResponseDTO>.Success(response, "Lấy chi tiết vaccine thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết vaccine với ID: {Id}", id);
                return ApiResult<VaccineTypeDetailResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineTypeResponseDTO>> CreateVaccineTypeAsync(CreateVaccineTypeRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate unique code
                    var existingVaccine = await _unitOfWork.VaccineTypeRepository.GetByCodeAsync(request.Code);
                    if (existingVaccine != null)
                    {
                        return ApiResult<VaccineTypeResponseDTO>.Failure(
                            new ArgumentException($"Mã vaccine '{request.Code}' đã tồn tại"));
                    }

                    var vaccineType = VaccineTypeMapper.MapFromCreateRequest(request);

                    // Create vaccine dose info (default: 1 dose)
                    var doseInfo = new VaccineDoseInfo
                    {
                        // Don't set Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy - BaseService will handle these
                        VaccineTypeId = vaccineType.Id,
                        DoseNumber = 1,
                        RecommendedAgeMonths = request.RecommendedAgeMonths,
                        MinIntervalDays = request.MinIntervalDays,
                        PreviousDoseId = null
                    };

                    vaccineType.VaccineDoseInfos.Add(doseInfo);

                    // BaseService sẽ tự động xử lý audit fields cho cả VaccinationType và VaccineDoseInfo
                    var createdVaccine = await CreateAsync(vaccineType);
                    var response = VaccineTypeMapper.MapToResponseDTO(createdVaccine);

                    _logger.LogInformation("Tạo vaccine mới thành công: {Code} - {Name}", request.Code, request.Name);
                    return ApiResult<VaccineTypeResponseDTO>.Success(response, "Tạo vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo vaccine: {Code}", request.Code);
                    return ApiResult<VaccineTypeResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccineTypeResponseDTO>> UpdateVaccineTypeAsync(Guid id, UpdateVaccineTypeRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var existingVaccine = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(id);
                    if (existingVaccine == null)
                    {
                        return ApiResult<VaccineTypeResponseDTO>.Failure(
                            new KeyNotFoundException($"Không tìm thấy vaccine với ID: {id}"));
                    }

                    // Validate unique code if changed
                    if (!string.IsNullOrEmpty(request.Code) && request.Code != existingVaccine.Code)
                    {
                        var codeExists = await _unitOfWork.VaccineTypeRepository.GetByCodeAsync(request.Code);
                        if (codeExists != null)
                        {
                            return ApiResult<VaccineTypeResponseDTO>.Failure(
                                new ArgumentException($"Mã vaccine '{request.Code}' đã tồn tại"));
                        }
                    }

                    VaccineTypeMapper.UpdateFromRequest(existingVaccine, request);

                    // BaseService sẽ tự động xử lý audit fields
                    var updatedVaccine = await UpdateAsync(existingVaccine);
                    var response = VaccineTypeMapper.MapToResponseDTO(updatedVaccine);

                    _logger.LogInformation("Cập nhật vaccine thành công: {Id}", id);
                    return ApiResult<VaccineTypeResponseDTO>.Success(response, "Cập nhật vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật vaccine với ID: {Id}", id);
                    return ApiResult<VaccineTypeResponseDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineTypesAsync(List<Guid> ids, bool isPermanent = false)
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
                    _logger.LogInformation("Starting batch {Operation} for {Count} vaccine types",
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

                    _logger.LogInformation("Batch {Operation} completed: {SuccessCount}/{TotalCount} vaccine types",
                        operationType, result.SuccessCount, result.TotalRequested);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thực hiện batch delete vaccine");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreVaccineTypesAsync(List<Guid> ids)
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

                    _logger.LogInformation("Starting batch restore for {Count} vaccine types", ids.Count);

                    var result = new BatchOperationResultDTO { TotalRequested = ids.Count };

                    var vaccineTypes = await _unitOfWork.VaccineTypeRepository.GetVaccineTypesByIdsAsync(ids, includeDeleted: true);
                    var foundIds = vaccineTypes.Select(v => v.Id).ToHashSet();

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
                                    Details = $"Vaccine với ID {id} không tồn tại"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            var vaccine = vaccineTypes.First(v => v.Id == id);

                            if (!vaccine.IsDeleted)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Trạng thái không hợp lệ",
                                    Details = "Vaccine chưa bị xóa"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            // Restore using proper approach
                            vaccine.IsDeleted = false;
                            vaccine.DeletedAt = null;
                            vaccine.DeletedBy = null;

                            // BaseService sẽ tự động xử lý UpdatedAt và UpdatedBy
                            await UpdateAsync(vaccine);

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
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

                    _logger.LogInformation("Batch restore completed: {SuccessCount}/{TotalCount} vaccine types",
                        result.SuccessCount, ids.Count);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi khôi phục vaccine");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<VaccineTypeResponseDTO>>> GetSoftDeletedVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var pagedVaccineTypes = await _unitOfWork.VaccineTypeRepository.GetSoftDeletedVaccineTypesAsync(
                    pageNumber, pageSize, searchTerm);

                var responseItems = pagedVaccineTypes.Select(VaccineTypeMapper.MapToResponseDTO).ToList();
                var pagedResult = VaccineTypeMapper.ToPagedResult(pagedVaccineTypes, responseItems);

                return ApiResult<PagedList<VaccineTypeResponseDTO>>.Success(
                    pagedResult,
                    $"Lấy danh sách vaccine đã xóa thành công. Tổng số: {pagedResult.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách vaccine đã xóa");
                return ApiResult<PagedList<VaccineTypeResponseDTO>>.Failure(ex);
            }
        }

        #endregion

        #region Business Operations

        public async Task<ApiResult<List<VaccineTypeResponseDTO>>> GetActiveVaccineTypesAsync()
        {
            try
            {
                var activeVaccines = await _unitOfWork.VaccineTypeRepository.GetActiveVaccineTypesAsync();
                var response = activeVaccines.Select(VaccineTypeMapper.MapToResponseDTO).ToList();

                return ApiResult<List<VaccineTypeResponseDTO>>.Success(
                    response, $"Lấy danh sách vaccine hoạt động thành công. Tổng số: {response.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách vaccine hoạt động");
                return ApiResult<List<VaccineTypeResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccineTypeResponseDTO>> ToggleVaccineTypeStatusAsync(Guid id)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var vaccine = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(id);
                    if (vaccine == null)
                    {
                        return ApiResult<VaccineTypeResponseDTO>.Failure(
                            new KeyNotFoundException($"Không tìm thấy vaccine với ID: {id}"));
                    }

                    vaccine.IsActive = !vaccine.IsActive;

                    // BaseService sẽ tự động xử lý audit fields
                    var updatedVaccine = await UpdateAsync(vaccine);
                    var response = VaccineTypeMapper.MapToResponseDTO(updatedVaccine);

                    var statusText = vaccine.IsActive ? "kích hoạt" : "vô hiệu hóa";
                    _logger.LogInformation("Đã {Status} vaccine: {Id}", statusText, id);

                    return ApiResult<VaccineTypeResponseDTO>.Success(
                        response, $"Đã {statusText} vaccine thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thay đổi trạng thái vaccine với ID: {Id}", id);
                    return ApiResult<VaccineTypeResponseDTO>.Failure(ex);
                }
            });
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
                return (false, $"Không thể {operation} quá 100 vaccine cùng lúc");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Process permanent delete operations
        /// </summary>
        private async Task ProcessPermanentDelete(List<Guid> ids, BatchOperationResultDTO result)
        {
            var vaccineTypes = await _unitOfWork.VaccineTypeRepository.GetVaccineTypesByIdsAsync(ids, includeDeleted: true);
            var foundIds = vaccineTypes.Select(v => v.Id).ToHashSet();

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
                            Details = $"Vaccine với ID {id} không tồn tại"
                        });
                        continue;
                    }

                    var vaccine = vaccineTypes.First(v => v.Id == id);

                    // Check dependencies before permanent delete
                    if (vaccine.Schedules?.Any() == true || vaccine.MedicationLots?.Any() == true)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Có ràng buộc dữ liệu",
                            Details = "Vaccine đã được sử dụng trong lịch tiêm hoặc lô thuốc"
                        });
                        continue;
                    }

                    await _unitOfWork.VaccineTypeRepository.DeleteAsync(id);
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
                    var vaccine = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(id);
                    if (vaccine == null)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Không tìm thấy",
                            Details = $"Vaccine với ID {id} không tồn tại"
                        });
                        continue;
                    }

                    if (vaccine.IsDeleted)
                    {
                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = id.ToString(),
                            Error = "Đã bị xóa",
                            Details = "Vaccine đã được xóa trước đó"
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
                            Details = "Không thể xóa vaccine"
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
        /// Generate batch operation message
        /// </summary>
        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công {result.SuccessCount} vaccine";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} vaccine. " +
                       $"{result.FailureCount} thất bại";

            return $"Không thể {operation} vaccine nào. Tất cả {result.FailureCount} yêu cầu đều thất bại";
        }

        #endregion
    }
}