using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class MedicationService : BaseService<Medication, Guid>, IMedicationService
    {
        private readonly IMedicationRepository _medicationRepository;
        private readonly ILogger<MedicationService> _logger;

        public MedicationService(
            IMedicationRepository medicationRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<MedicationService> logger,
            ICurrentTime currentTime)
            : base(medicationRepository, currentUserService, unitOfWork, currentTime)
        {
            _medicationRepository = medicationRepository;
            _logger = logger;
        }

        #region Public API Methods

        /// <summary>
        /// Lấy danh sách thuốc theo phân trang, có thể lọc theo searchTerm, category và includeDeleted.
        /// </summary>
        public async Task<ApiResult<PagedList<MedicationResponse>>> GetMedicationsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            MedicationCategory? category = null,
            bool includeDeleted = false)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var currentUserId = _currentUserService.GetUserId();

            try
            {
                _logger.LogInformation(
                    "GetMedicationsAsync started by user {UserId} at {Timestamp} - Page: {PageNumber}, Size: {PageSize}, " +
                    "Search: '{SearchTerm}', Category: {Category}, IncludeDeleted: {IncludeDeleted}",
                    currentUserId, DateTime.UtcNow, pageNumber, pageSize, searchTerm, category, includeDeleted);

                // Validate input parameters
                var validationResult = ValidatePagingParameters(pageNumber, pageSize);
                if (!validationResult.IsValid)
                {
                    return ApiResult<PagedList<MedicationResponse>>.Failure(
                        new ArgumentException(validationResult.ErrorMessage));
                }

                // Normalize search term
                var normalizedSearchTerm = NormalizeSearchTerm(searchTerm);

                // Get medications from repository
                var medicationsPaged = await GetMedicationsFromRepository(
                    pageNumber, pageSize, normalizedSearchTerm, category, includeDeleted);

                // Check if any medications found
                if (medicationsPaged == null || !medicationsPaged.Any())
                {
                    return CreateEmptyResult(pageNumber, pageSize, normalizedSearchTerm, category, includeDeleted);
                }

                // Map to response DTOs using batch loading
                var medicationResponses = await MapToMedicationResponsesAsync(medicationsPaged);

                // Create paged result
                var pagedResult = new PagedList<MedicationResponse>(
                    medicationResponses,
                    medicationsPaged.MetaData.TotalCount,
                    pageNumber,
                    pageSize);

                // Generate success message and log metrics
                var message = GenerateGetMedicationsSuccessMessage(pagedResult, normalizedSearchTerm, category, includeDeleted);
                LogSuccessMetrics(stopwatch, pagedResult, currentUserId);

                return ApiResult<PagedList<MedicationResponse>>.Success(pagedResult, message);
            }
            catch (ArgumentException argEx)
            {
                LogWarning(stopwatch, argEx, currentUserId);
                return ApiResult<PagedList<MedicationResponse>>.Failure(argEx);
            }
            catch (Exception ex)
            {
                LogError(stopwatch, ex, currentUserId, pageNumber, pageSize, searchTerm, category, includeDeleted);
                return ApiResult<PagedList<MedicationResponse>>.Failure(
                    new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc. Vui lòng thử lại sau."));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết thuốc theo Id.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> GetMedicationByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return ApiResult<MedicationResponse>.Failure(
                        new ArgumentException("ID thuốc không hợp lệ"));
                }

                var medication = await _medicationRepository.GetByIdAsync(id, m => m.Lots);

                if (medication == null || medication.IsDeleted)
                {
                    return ApiResult<MedicationResponse>.Failure(
                        new Exception("Không tìm thấy thuốc"));
                }

                var response = await MapSingleMedicationToResponseAsync(medication);
                return ApiResult<MedicationResponse>.Success(response, "Lấy thông tin thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medication by id: {MedicationId}", id);
                return ApiResult<MedicationResponse>.Failure(
                    new Exception("Đã xảy ra lỗi khi lấy thông tin thuốc"));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết thuốc kèm thông tin lô.
        /// </summary>
        public async Task<ApiResult<MedicationDetailResponse>> GetMedicationDetailByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return ApiResult<MedicationDetailResponse>.Failure(
                        new ArgumentException("ID thuốc không hợp lệ"));
                }

                var medication = await _medicationRepository.GetByIdAsync(id, m => m.Lots);

                if (medication == null || medication.IsDeleted)
                {
                    return ApiResult<MedicationDetailResponse>.Failure(
                        new Exception("Không tìm thấy thuốc"));
                }

                var lots = await _unitOfWork.MedicationLotRepository.GetLotsByMedicationIdAsync(id);
                var response = MapToMedicationDetailResponse(medication, lots);

                return ApiResult<MedicationDetailResponse>.Success(response, "Lấy thông tin chi tiết thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medication detail by id: {MedicationId}", id);
                return ApiResult<MedicationDetailResponse>.Failure(
                    new Exception("Đã xảy ra lỗi khi lấy thông tin chi tiết thuốc"));
            }
        }

        /// <summary>
        /// Tạo mới một thuốc.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> CreateMedicationAsync(CreateMedicationRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate request
                    var validationResult = await ValidateCreateRequestAsync(request);
                    if (!validationResult.IsValid)
                    {
                        return ApiResult<MedicationResponse>.Failure(
                            new Exception(validationResult.ErrorMessage));
                    }

                    // Create entity and save
                    var medication = MapToMedicationEntity(request);
                    var createdMedication = await CreateAsync(medication);

                    // Map to response
                    var response = await MapSingleMedicationToResponseAsync(createdMedication);

                    _logger.LogInformation("Medication created successfully: {MedicationId}", createdMedication.Id);
                    return ApiResult<MedicationResponse>.Success(response, "Tạo thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating medication");
                    return ApiResult<MedicationResponse>.Failure(
                        new Exception("Đã xảy ra lỗi khi tạo thuốc"));
                }
            });
        }

        /// <summary>
        /// Cập nhật thông tin thuốc theo Id.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> UpdateMedicationAsync(
            Guid id, UpdateMedicationRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (id == Guid.Empty)
                    {
                        return ApiResult<MedicationResponse>.Failure(
                            new ArgumentException("ID thuốc không hợp lệ"));
                    }

                    var medication = await _medicationRepository.GetByIdAsync(id, m => m.Lots);

                    if (medication == null || medication.IsDeleted)
                    {
                        return ApiResult<MedicationResponse>.Failure(
                            new Exception("Không tìm thấy thuốc"));
                    }

                    // Validate update request
                    var validationResult = await ValidateUpdateRequestAsync(request, id);
                    if (!validationResult.IsValid)
                    {
                        return ApiResult<MedicationResponse>.Failure(
                            new Exception(validationResult.ErrorMessage));
                    }

                    // Update entity
                    UpdateMedicationEntity(medication, request);
                    var updatedMedication = await UpdateAsync(medication);

                    // Map to response
                    var response = await MapSingleMedicationToResponseAsync(updatedMedication);

                    _logger.LogInformation("Medication updated successfully: {MedicationId}", id);
                    return ApiResult<MedicationResponse>.Success(response, "Cập nhật thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating medication: {MedicationId}", id);
                    return ApiResult<MedicationResponse>.Failure(
                        new Exception("Đã xảy ra lỗi khi cập nhật thuốc"));
                }
            });
        }

        /// <summary>
        /// Xóa thuốc (hỗ trợ xóa 1 hoặc nhiều, soft delete hoặc permanent).
        /// </summary>
        public async Task<ApiResult<BatchOperationResultDTO>> DeleteMedicationsAsync(List<Guid> medicationIds, bool isPermanent = false)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchOperationIds(medicationIds);
                    if (!validationResult.IsValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.ErrorMessage));
                    }

                    var result = await ExecuteBatchDeleteOperation(medicationIds, isPermanent);
                    await _unitOfWork.SaveChangesAsync();

                    var actionType = isPermanent ? "xóa vĩnh viễn" : "xóa";
                    result.Message = GenerateBatchOperationMessage(result, actionType);

                    LogBatchOperationResult(actionType, result);

                    return result.SuccessCount > 0
                        ? ApiResult<BatchOperationResultDTO>.Success(result, result.Message)
                        : ApiResult<BatchOperationResultDTO>.Failure(new Exception(result.Message));
                }
                catch (Exception ex)
                {
                    var actionType = isPermanent ? "permanently deleting" : "soft deleting";
                    _logger.LogError(ex, "Error occurred while {ActionType} medications", actionType);
                    return ApiResult<BatchOperationResultDTO>.Failure(
                        new Exception("Đã xảy ra lỗi khi xóa thuốc"));
                }
            });
        }

        /// <summary>
        /// Khôi phục thuốc đã bị soft delete (hỗ trợ 1 hoặc nhiều).
        /// </summary>
        public async Task<ApiResult<BatchOperationResultDTO>> RestoreMedicationsAsync(List<Guid> medicationIds)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchOperationIds(medicationIds);
                    if (!validationResult.IsValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.ErrorMessage));
                    }

                    var result = await ExecuteBatchRestoreOperation(medicationIds);
                    await _unitOfWork.SaveChangesAsync();

                    result.Message = GenerateBatchOperationMessage(result, "khôi phục");
                    LogBatchOperationResult("khôi phục", result);

                    return result.SuccessCount > 0
                        ? ApiResult<BatchOperationResultDTO>.Success(result, result.Message)
                        : ApiResult<BatchOperationResultDTO>.Failure(new Exception(result.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while restoring medications");
                    return ApiResult<BatchOperationResultDTO>.Failure(
                        new Exception("Đã xảy ra lỗi khi khôi phục thuốc"));
                }
            });
        }

        /// <summary>
        /// Lấy danh sách thuốc đang ở trạng thái Active.
        /// </summary>
        public async Task<ApiResult<List<MedicationResponse>>> GetActiveMedicationsAsync()
        {
            try
            {
                var medications = await _medicationRepository.GetActiveMedicationsAsync();
                var responses = await MapToMedicationResponsesAsync(medications);

                return ApiResult<List<MedicationResponse>>.Success(
                    responses, "Lấy danh sách thuốc đang hoạt động thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active medications");
                return ApiResult<List<MedicationResponse>>.Failure(
                    new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc đang hoạt động"));
            }
        }

        /// <summary>
        /// Xóa vĩnh viễn các thuốc đã soft delete quá thời hạn.
        /// </summary>
        public async Task<ApiResult<string>> CleanupExpiredMedicationsAsync(int daysToExpire = 30)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    if (daysToExpire <= 0)
                    {
                        return ApiResult<string>.Failure(
                            new ArgumentException("Số ngày hết hạn phải lớn hơn 0"));
                    }

                    var deletedCount = await _medicationRepository.PermanentDeleteExpiredAsync(daysToExpire);
                    await _unitOfWork.SaveChangesAsync();

                    var message = deletedCount > 0
                        ? $"Đã xóa vĩnh viễn {deletedCount} thuốc hết hạn"
                        : "Không có thuốc nào cần xóa";

                    _logger.LogInformation("Cleanup expired medications completed: {DeletedCount} medications deleted", deletedCount);
                    return ApiResult<string>.Success(message, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired medications");
                    return ApiResult<string>.Failure(
                        new Exception("Đã xảy ra lỗi khi dọn dẹp thuốc hết hạn"));
                }
            });
        }

        #endregion

        #region Private Helper Methods

        #region Validation Methods

        private static (bool IsValid, string ErrorMessage) ValidatePagingParameters(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                return (false, "Số trang phải lớn hơn 0");

            if (pageSize < 1 || pageSize > 100)
                return (false, "Kích thước trang phải từ 1 đến 100");

            return (true, string.Empty);
        }

        private static (bool IsValid, string ErrorMessage) ValidateBatchOperationIds(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return (false, "Danh sách ID không được rỗng");

            if (ids.Any(id => id == Guid.Empty))
                return (false, "Danh sách chứa ID không hợp lệ");

            if (ids.Count > 100)
                return (false, "Không thể xử lý quá 100 thuốc cùng lúc");

            return (true, string.Empty);
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateCreateRequestAsync(CreateMedicationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return (false, "Tên thuốc không được để trống");

            if (await _medicationRepository.MedicationNameExistsAsync(request.Name))
                return (false, "Tên thuốc đã tồn tại");

            return (true, string.Empty);
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateUpdateRequestAsync(UpdateMedicationRequest request, Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return (false, "Tên thuốc không được để trống");

            if (await _medicationRepository.MedicationNameExistsAsync(request.Name, excludeId))
                return (false, "Tên thuốc đã tồn tại");

            return (true, string.Empty);
        }

        #endregion

        #region Repository Access Methods

        private async Task<PagedList<Medication>> GetMedicationsFromRepository(
            int pageNumber, int pageSize, string? searchTerm, MedicationCategory? category, bool includeDeleted)
        {
            if (includeDeleted)
            {
                _logger.LogInformation("Fetching medications including deleted ones");
                return await _medicationRepository.GetAllMedicationsIncludingDeletedAsync(
                    pageNumber, pageSize, searchTerm, category);
            }
            else
            {
                _logger.LogInformation("Fetching active medications only");
                return await _medicationRepository.GetMedicationsAsync(
                    pageNumber, pageSize, searchTerm, category);
            }
        }

        #endregion

        #region Batch Operations

        private async Task<BatchOperationResultDTO> ExecuteBatchDeleteOperation(List<Guid> medicationIds, bool isPermanent)
        {
            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
            var result = new BatchOperationResultDTO { TotalRequested = medicationIds.Count };

            foreach (var id in medicationIds)
            {
                try
                {
                    bool success = isPermanent
                        ? await _medicationRepository.PermanentDeleteWithLotsAsync(id)
                        : await _medicationRepository.SoftDeleteWithLotsAsync(id, currentUserId);

                    if (success)
                    {
                        result.SuccessCount++;
                        result.SuccessIds.Add(id.ToString());
                    }
                    else
                    {
                        AddBatchOperationError(result, id, "NotFound", "Không tìm thấy thuốc hoặc thuốc đã bị xóa");
                    }
                }
                catch (Exception ex)
                {
                    AddBatchOperationError(result, id, "Exception", ex.Message);
                    _logger.LogError(ex, "Error deleting medication {MedicationId}", id);
                }
            }

            return result;
        }

        private async Task<BatchOperationResultDTO> ExecuteBatchRestoreOperation(List<Guid> medicationIds)
        {
            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
            var result = new BatchOperationResultDTO { TotalRequested = medicationIds.Count };

            foreach (var id in medicationIds)
            {
                try
                {
                    var success = await _medicationRepository.RestoreWithLotsAsync(id, currentUserId);

                    if (success)
                    {
                        result.SuccessCount++;
                        result.SuccessIds.Add(id.ToString());
                    }
                    else
                    {
                        AddBatchOperationError(result, id, "NotFound", "Không tìm thấy thuốc đã bị xóa");
                    }
                }
                catch (Exception ex)
                {
                    AddBatchOperationError(result, id, "Exception", ex.Message);
                    _logger.LogError(ex, "Error restoring medication {MedicationId}", id);
                }
            }

            return result;
        }

        private static void AddBatchOperationError(BatchOperationResultDTO result, Guid id, string error, string details)
        {
            result.FailureCount++;
            result.Errors.Add(new BatchOperationErrorDTO
            {
                Id = id.ToString(),
                Error = error,
                Details = details
            });
        }

        #endregion

        #region Mapping Methods - OPTIMIZED BATCH LOADING

        /// <summary>
        /// Map collection of Medication entities to MedicationResponses using batch loading approach.
        /// This method eliminates DbContext concurrency issues by loading all required data upfront.
        /// </summary>
        private async Task<List<MedicationResponse>> MapToMedicationResponsesAsync(IEnumerable<Medication> medications)
        {
            try
            {
                var medicationList = medications.ToList();

                if (!medicationList.Any())
                {
                    return new List<MedicationResponse>();
                }

                var currentUserId = _currentUserService.GetUserId();

                _logger.LogDebug("Mapping {Count} medications to responses for user {UserId}",
                    medicationList.Count, currentUserId);

                // 🎯 STEP 1: Batch load all required data in single queries
                var medicationIds = medicationList.Select(m => m.Id).ToList();

                var totalQuantities = await GetTotalQuantitiesByMedicationIdsAsync(medicationIds);
                var lotCounts = await GetLotCountsByMedicationIdsAsync(medicationIds);

                // 🎯 STEP 2: Map synchronously using pre-loaded data
                var responses = medicationList.Select(medication =>
                {
                    try
                    {
                        var totalQuantity = totalQuantities.GetValueOrDefault(medication.Id, 0);
                        var totalLots = lotCounts.GetValueOrDefault(medication.Id, 0);

                        return MapToMedicationResponse(medication, totalQuantity, totalLots);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error mapping medication {MedicationId} to response", medication.Id);
                        return CreateFallbackMedicationResponse(medication);
                    }
                }).ToList();

                _logger.LogDebug("Successfully mapped {Count} medications to responses for user {UserId}",
                    responses.Count, currentUserId);

                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in MapToMedicationResponsesAsync for user {UserId}",
                    _currentUserService.GetUserId());
                throw new Exception("Lỗi xử lý dữ liệu thuốc", ex);
            }
        }

        /// <summary>
        /// Map single medication to response (for GetById scenarios)
        /// </summary>
        private async Task<MedicationResponse> MapSingleMedicationToResponseAsync(Medication medication)
        {
            try
            {
                var totalQuantity = await _medicationRepository.GetTotalQuantityByMedicationIdAsync(medication.Id);
                var totalLots = medication.Lots?.Count ?? 0;

                return MapToMedicationResponse(medication, totalQuantity, totalLots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping single medication {MedicationId} to response", medication.Id);
                return CreateFallbackMedicationResponse(medication);
            }
        }

        /// <summary>
        /// Batch load total quantities for multiple medications
        /// </summary>
        private async Task<Dictionary<Guid, int>> GetTotalQuantitiesByMedicationIdsAsync(List<Guid> medicationIds)
        {
            try
            {
                return await _medicationRepository.GetTotalQuantitiesByMedicationIdsAsync(medicationIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total quantities for medications");
                return new Dictionary<Guid, int>();
            }
        }

        /// <summary>
        /// Batch load lot counts for multiple medications
        /// </summary>
        private async Task<Dictionary<Guid, int>> GetLotCountsByMedicationIdsAsync(List<Guid> medicationIds)
        {
            try
            {
                return await _medicationRepository.GetLotCountsByMedicationIdsAsync(medicationIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lot counts for medications");
                return new Dictionary<Guid, int>();
            }
        }

        /// <summary>
        /// Core mapping method - synchronous, no database calls
        /// </summary>
        private static MedicationResponse MapToMedicationResponse(Medication medication, int totalQuantity, int totalLots)
        {
            return new MedicationResponse
            {
                Id = medication.Id,
                Name = medication.Name,
                Unit = medication.Unit ?? "N/A",
                DosageForm = medication.DosageForm ?? "N/A",
                Category = medication.Category,
                Status = medication.Status,
                CreatedAt = medication.CreatedAt,
                UpdatedAt = medication.UpdatedAt,
                TotalLots = totalLots,
                TotalQuantity = totalQuantity
            };
        }

        /// <summary>
        /// Create fallback response when mapping fails
        /// </summary>
        private static MedicationResponse CreateFallbackMedicationResponse(Medication medication)
        {
            return new MedicationResponse
            {
                Id = medication.Id,
                Name = medication.Name,
                Unit = medication.Unit ?? "N/A",
                DosageForm = medication.DosageForm ?? "N/A",
                Category = medication.Category,
                Status = medication.Status,
                CreatedAt = medication.CreatedAt,
                UpdatedAt = medication.UpdatedAt,
                TotalLots = 0,
                TotalQuantity = 0
            };
        }

        /// <summary>
        /// Map medication with lots to detailed response
        /// </summary>
        private static MedicationDetailResponse MapToMedicationDetailResponse(Medication medication, List<MedicationLot> lots)
        {
            return new MedicationDetailResponse
            {
                Id = medication.Id,
                Name = medication.Name,
                Unit = medication.Unit,
                DosageForm = medication.DosageForm,
                Category = medication.Category.ToString(),
                Status = medication.Status.ToString(),
                CreatedAt = medication.CreatedAt,
                UpdatedAt = medication.UpdatedAt,
                TotalLots = lots.Count,
                TotalQuantity = lots.Sum(l => l.Quantity),
                Lots = lots.Select(lot => new MedicationLotDetailResponse
                {
                    Id = lot.Id,
                    LotNumber = lot.LotNumber,
                    ExpiryDate = lot.ExpiryDate,
                    Quantity = lot.Quantity,
                    StorageLocation = lot.StorageLocation,
                    CreatedAt = lot.CreatedAt,
                    UpdatedAt = lot.UpdatedAt
                }).OrderBy(l => l.ExpiryDate).ToList()
            };
        }

        /// <summary>
        /// Map CreateRequest to Entity
        /// </summary>
        private static Medication MapToMedicationEntity(CreateMedicationRequest request)
        {
            return new Medication
            {
                Name = request.Name,
                Unit = request.Unit,
                DosageForm = request.DosageForm,
                Category = request.Category,
                Status = request.Status,
                IsDeleted = false
            };
        }

        /// <summary>
        /// Update entity from UpdateRequest
        /// </summary>
        private static void UpdateMedicationEntity(Medication medication, UpdateMedicationRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
                medication.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Unit))
                medication.Unit = request.Unit;

            if (!string.IsNullOrWhiteSpace(request.DosageForm))
                medication.DosageForm = request.DosageForm;

            if (request.Category.HasValue)
                medication.Category = request.Category.Value;

            if (request.Status.HasValue)
                medication.Status = request.Status.Value;
        }

        #endregion

        #region Utility Methods

        private static string? NormalizeSearchTerm(string? searchTerm)
        {
            return string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim().ToLowerInvariant();
        }

        private ApiResult<PagedList<MedicationResponse>> CreateEmptyResult(
            int pageNumber, int pageSize, string? searchTerm, MedicationCategory? category, bool includeDeleted)
        {
            _logger.LogInformation(
                "No medications found with criteria - Search: '{SearchTerm}', Category: {Category}, IncludeDeleted: {IncludeDeleted}",
                searchTerm, category, includeDeleted);

            var emptyResult = new PagedList<MedicationResponse>(
                new List<MedicationResponse>(), 0, pageNumber, pageSize);

            return ApiResult<PagedList<MedicationResponse>>.Success(
                emptyResult, "Không tìm thấy thuốc nào phù hợp với tiêu chí tìm kiếm");
        }

        private static string GenerateBatchOperationMessage(BatchOperationResultDTO result, string actionType)
        {
            if (result.IsCompleteSuccess)
            {
                return $"Đã {actionType} thành công tất cả {result.TotalRequested} thuốc";
            }
            else if (result.IsCompleteFailure)
            {
                return $"Không thể {actionType} bất kỳ thuốc nào";
            }
            else if (result.IsPartialSuccess)
            {
                return $"Đã {actionType} thành công {result.SuccessCount}/{result.TotalRequested} thuốc. " +
                       $"Không thể {actionType} {result.FailureCount} thuốc";
            }

            return $"Hoàn thành việc {actionType} thuốc";
        }

        private static string GenerateGetMedicationsSuccessMessage(
            PagedList<MedicationResponse> pagedResult,
            string? searchTerm,
            MedicationCategory? category,
            bool includeDeleted)
        {
            var totalCount = pagedResult.MetaData.TotalCount;
            var currentPage = pagedResult.MetaData.CurrentPage;
            var totalPages = pagedResult.MetaData.TotalPages;

            var baseMessage = $"Lấy danh sách thuốc thành công: {totalCount} thuốc";

            if (totalPages > 1)
            {
                baseMessage += $" (trang {currentPage}/{totalPages})";
            }

            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                conditions.Add($"từ khóa '{searchTerm}'");
            }

            if (category.HasValue)
            {
                conditions.Add($"danh mục {GetCategoryDisplayName(category.Value)}");
            }

            if (includeDeleted)
            {
                conditions.Add("bao gồm thuốc đã xóa");
            }

            if (conditions.Any())
            {
                baseMessage += $" với điều kiện: {string.Join(", ", conditions)}";
            }

            return baseMessage;
        }

        private static string GetCategoryDisplayName(MedicationCategory category)
        {
            return category switch
            {
                MedicationCategory.Emergency => "Thuốc cấp cứu",
                MedicationCategory.PainRelief => "Thuốc giảm đau",
                MedicationCategory.AntiAllergy => "Thuốc chống dị ứng",
                MedicationCategory.Antibiotic => "Thuốc kháng sinh",
                MedicationCategory.TopicalTreatment => "Thuốc bôi ngoài da",
                MedicationCategory.Disinfectant => "Thuốc sát trùng",
                MedicationCategory.SolutionAndVitamin => "Dung dịch và vitamin",
                MedicationCategory.Digestive => "Thuốc tiêu hóa",
                MedicationCategory.ENT => "Thuốc tai mũi họng",
                MedicationCategory.Respiratory => "Thuốc hô hấp",
                _ => category.ToString()
            };
        }

        #endregion

        #region Logging Methods

        private void LogSuccessMetrics(System.Diagnostics.Stopwatch stopwatch,
            PagedList<MedicationResponse> pagedResult, Guid? currentUserId)
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "GetMedicationsAsync completed successfully - User: {UserId}, Duration: {Duration}ms, " +
                "TotalCount: {TotalCount}, PageCount: {PageCount}, CurrentPage: {CurrentPage}",
                currentUserId, stopwatch.ElapsedMilliseconds, pagedResult.MetaData.TotalCount,
                pagedResult.MetaData.TotalPages, pagedResult.MetaData.CurrentPage);
        }

        private void LogWarning(System.Diagnostics.Stopwatch stopwatch, ArgumentException argEx, Guid? currentUserId)
        {
            stopwatch.Stop();
            _logger.LogWarning(argEx,
                "Invalid argument in GetMedicationsAsync by user {UserId} - Duration: {Duration}ms",
                currentUserId, stopwatch.ElapsedMilliseconds);
        }

        private void LogError(System.Diagnostics.Stopwatch stopwatch, Exception ex, Guid? currentUserId,
            int pageNumber, int pageSize, string? searchTerm, MedicationCategory? category, bool includeDeleted)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Error occurred while getting medications by user {UserId} - Duration: {Duration}ms, " +
                "Parameters: PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm='{SearchTerm}', " +
                "Category={Category}, IncludeDeleted={IncludeDeleted}",
                currentUserId, stopwatch.ElapsedMilliseconds, pageNumber, pageSize,
                searchTerm, category, includeDeleted);
        }

        private void LogBatchOperationResult(string actionType, BatchOperationResultDTO result)
        {
            var currentUserId = _currentUserService.GetUserId();
            _logger.LogInformation("Medications {ActionType}: Success={SuccessCount}, Failed={FailedCount} by user: {UserId}",
                actionType, result.SuccessCount, result.FailureCount, currentUserId);
        }

        #endregion

        #endregion
    }
}