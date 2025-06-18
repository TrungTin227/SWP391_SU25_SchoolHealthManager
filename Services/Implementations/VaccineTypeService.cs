using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Commons;
using Services.Helpers.Mappers;

namespace Services.Implementations
{
    public class VaccineTypeService : BaseService<VaccinationType, Guid>, IVaccineTypeService
    {
        private readonly IVaccineTypeRepository _vaccineTypeRepository;
        private readonly ILogger<VaccineTypeService> _logger;

        public VaccineTypeService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccineTypeService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccineTypeRepository, currentUserService, unitOfWork, currentTime)
        {
            _vaccineTypeRepository = unitOfWork.VaccineTypeRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<VaccineTypeResponseDTO>>> GetVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            try
            {
                var pagedVaccineTypes = await _vaccineTypeRepository.GetVaccineTypesAsync(
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
                var vaccineType = await _vaccineTypeRepository.GetVaccineTypeByIdAsync(id);
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
                var vaccineType = await _vaccineTypeRepository.GetVaccineTypeWithDetailsAsync(id);
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
            try
            {
                // Validate unique code
                var existingVaccine = await _vaccineTypeRepository.GetByCodeAsync(request.Code);
                if (existingVaccine != null)
                {
                    return ApiResult<VaccineTypeResponseDTO>.Failure(
                        new InvalidOperationException($"Mã vaccine '{request.Code}' đã tồn tại"));
                }

                var vaccineType = VaccineTypeMapper.MapFromCreateRequest(request);

                // Create vaccine dose info (default: 1 dose)
                var doseInfo = new VaccineDoseInfo
                {
                    Id = Guid.NewGuid(),
                    VaccineTypeId = vaccineType.Id,
                    DoseNumber = 1,
                    RecommendedAgeMonths = request.RecommendedAgeMonths,
                    MinIntervalDays = request.MinIntervalDays, // ✅ FIX: Sử dụng từ request
                    PreviousDoseId = null
                };

                vaccineType.VaccineDoseInfos.Add(doseInfo);

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
        }

        public async Task<ApiResult<VaccineTypeResponseDTO>> UpdateVaccineTypeAsync(Guid id, UpdateVaccineTypeRequest request)
        {
            try
            {
                var existingVaccine = await _vaccineTypeRepository.GetByIdAsync(id);
                if (existingVaccine == null)
                {
                    return ApiResult<VaccineTypeResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy vaccine với ID: {id}"));
                }

                // Validate unique code if changed
                if (!string.IsNullOrEmpty(request.Code) && request.Code != existingVaccine.Code)
                {
                    var codeExists = await _vaccineTypeRepository.GetByCodeAsync(request.Code);
                    if (codeExists != null)
                    {
                        return ApiResult<VaccineTypeResponseDTO>.Failure(
                            new InvalidOperationException($"Mã vaccine '{request.Code}' đã tồn tại"));
                    }
                }

                VaccineTypeMapper.UpdateFromRequest(existingVaccine, request);
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
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineTypesAsync(List<Guid> ids, bool isPermanent = false)
        {
            var result = new BatchOperationResultDTO
            {
                TotalRequested = ids.Count
            };

            try
            {
                var vaccineTypes = await _vaccineTypeRepository.GetVaccineTypesByIdsAsync(ids, includeDeleted: isPermanent);
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

                        if (isPermanent)
                        {
                            // Check dependencies before permanent delete
                            if (vaccine.Schedules.Any() || vaccine.MedicationLots.Any())
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Có ràng buộc dữ liệu",
                                    Details = "Vaccine đã được sử dụng trong lịch tiêm hoặc lô thuốc"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            await _repository.DeleteAsync(vaccine.Id);
                        }
                        else
                        {
                            await DeleteAsync(id);
                        }

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

                result.Message = GenerateBatchOperationMessage(isPermanent ? "xóa vĩnh viễn" : "xóa", result);
                return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện batch delete vaccine");
                return ApiResult<BatchOperationResultDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreVaccineTypesAsync(List<Guid> ids)
        {
            var result = new BatchOperationResultDTO
            {
                TotalRequested = ids.Count
            };

            try
            {
                var vaccineTypes = await _vaccineTypeRepository.GetVaccineTypesByIdsAsync(ids, includeDeleted: true);
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

                        // ✅ FIX: Restore manually
                        vaccine.IsDeleted = false;
                        vaccine.DeletedAt = null;
                        vaccine.DeletedBy = null;

                        var updatedVaccine = await UpdateAsync(vaccine);

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
                return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khôi phục vaccine");
                return ApiResult<BatchOperationResultDTO>.Failure(ex);
            }
        }

        #endregion

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<VaccineTypeResponseDTO>>> GetSoftDeletedVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var pagedVaccineTypes = await _vaccineTypeRepository.GetSoftDeletedVaccineTypesAsync(
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
                var activeVaccines = await _vaccineTypeRepository.GetActiveVaccineTypesAsync();
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
            try
            {
                var vaccine = await _vaccineTypeRepository.GetByIdAsync(id);
                if (vaccine == null)
                {
                    return ApiResult<VaccineTypeResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy vaccine với ID: {id}"));
                }

                vaccine.IsActive = !vaccine.IsActive;
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
        }

        #endregion

        #region Private Helper Methods

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