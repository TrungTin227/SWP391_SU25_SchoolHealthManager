using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccineDoseInfoService : BaseService<VaccineDoseInfo, Guid>, IVaccineDoseInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VaccineDoseInfoService> _logger;

        public VaccineDoseInfoService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<VaccineDoseInfoService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.VaccineDoseInfoRepository, currentUserService, unitOfWork, currentTime)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
                        new InvalidOperationException($"Mũi tiêm số {request.DoseNumber} đã tồn tại cho vaccine {vaccineType.Name}"));
                }

                // Validate previous dose if specified
                if (request.PreviousDoseId.HasValue)
                {
                    var previousDose = await _unitOfWork.VaccineDoseInfoRepository.GetByIdAsync(request.PreviousDoseId.Value);
                    if (previousDose == null || previousDose.VaccineTypeId != request.VaccineTypeId)
                    {
                        return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                            new InvalidOperationException("Mũi tiêm trước không hợp lệ"));
                    }

                    if (previousDose.DoseNumber >= request.DoseNumber)
                    {
                        return ApiResult<VaccineDoseInfoResponseDTO>.Failure(
                            new InvalidOperationException("Số mũi tiêm phải lớn hơn mũi tiêm trước"));
                    }
                }

                var doseInfo = VaccineDoseInfoMapper.MapFromCreateRequest(request);
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
        }

        public async Task<ApiResult<VaccineDoseInfoResponseDTO>> UpdateVaccineDoseInfoAsync(Guid id, UpdateVaccineDoseInfoRequest request)
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
                            new InvalidOperationException($"Mũi tiêm số {request.DoseNumber.Value} đã tồn tại"));
                    }
                }

                VaccineDoseInfoMapper.UpdateFromRequest(existingDoseInfo, request);
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
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteVaccineDoseInfosAsync(List<Guid> ids, bool isPermanent = false)
        {
            var result = new BatchOperationResultDTO
            {
                TotalRequested = ids.Count
            };

            try
            {
                var doseInfos = await _unitOfWork.VaccineDoseInfoRepository.GetVaccineDoseInfosByIdsAsync(ids, includeDeleted: isPermanent);
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
                            result.FailureCount++;
                            continue;
                        }

                        var doseInfo = doseInfos.First(d => d.Id == id);

                        // Check if this is the only dose for the vaccine type
                        var totalDoses = await _unitOfWork.VaccineDoseInfoRepository.GetDoseInfosByVaccineTypeAsync(doseInfo.VaccineTypeId);
                        if (totalDoses.Count == 1 && totalDoses.First().Id == id)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = id.ToString(),
                                Error = "Không thể xóa",
                                Details = "Không thể xóa mũi tiêm duy nhất của loại vaccine"
                            });
                            result.FailureCount++;
                            continue;
                        }

                        // Check if other doses depend on this dose
                        var nextDoses = await _unitOfWork.VaccineDoseInfoRepository.GetNextDosesAsync(id);
                        if (nextDoses.Any())
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = id.ToString(),
                                Error = "Có ràng buộc dữ liệu",
                                Details = $"Có {nextDoses.Count} mũi tiêm kế tiếp phụ thuộc vào mũi tiêm này"
                            });
                            result.FailureCount++;
                            continue;
                        }

                        if (isPermanent)
                        {
                            await _unitOfWork.VaccineDoseInfoRepository.DeleteAsync(doseInfo.Id);
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
                _logger.LogError(ex, "Lỗi khi thực hiện batch delete thông tin liều vaccine");
                return ApiResult<BatchOperationResultDTO>.Failure(ex);
            }
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