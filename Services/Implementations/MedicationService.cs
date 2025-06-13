using DTOs.MedicationDTOs.Request;
using DTOs.MedicationDTOs.Response;
using DTOs.MedicationLotDTOs.Response;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Commons;

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
                if (pageNumber < 1)
                {
                    _logger.LogWarning("Invalid pageNumber: {PageNumber} by user {UserId}", pageNumber, currentUserId);
                    return ApiResult<PagedList<MedicationResponse>>.Failure(
                        new ArgumentException("Số trang phải lớn hơn 0"));
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    _logger.LogWarning("Invalid pageSize: {PageSize} by user {UserId}", pageSize, currentUserId);
                    return ApiResult<PagedList<MedicationResponse>>.Failure(
                        new ArgumentException("Kích thước trang phải từ 1 đến 100"));
                }

                // Normalize search term
                var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm)
                    ? null
                    : searchTerm.Trim().ToLowerInvariant();

                // Get medications from repository based on includeDeleted flag
                PagedList<Medication> medicationsPaged;

                if (includeDeleted)
                {
                    _logger.LogInformation("Fetching medications including deleted ones for user {UserId}", currentUserId);
                    medicationsPaged = await _medicationRepository.GetAllMedicationsIncludingDeletedAsync(
                        pageNumber, pageSize, normalizedSearchTerm, category);
                }
                else
                {
                    _logger.LogInformation("Fetching active medications only for user {UserId}", currentUserId);
                    medicationsPaged = await _medicationRepository.GetMedicationsAsync(
                        pageNumber, pageSize, normalizedSearchTerm, category);
                }

                // Check if any medications found
                if (medicationsPaged == null || !medicationsPaged.Any())
                {
                    _logger.LogInformation(
                        "No medications found with criteria - User: {UserId}, Search: '{SearchTerm}', Category: {Category}, " +
                        "IncludeDeleted: {IncludeDeleted}",
                        currentUserId, normalizedSearchTerm, category, includeDeleted);

                    var emptyResult = new PagedList<MedicationResponse>(
                        new List<MedicationResponse>(), 0, pageNumber, pageSize);

                    return ApiResult<PagedList<MedicationResponse>>.Success(
                        emptyResult, "Không tìm thấy thuốc nào phù hợp với tiêu chí tìm kiếm");
                }

                // Map to response DTOs
                var medicationResponses = await MapToMedicationResponsesAsync(medicationsPaged);

                // Create paged result
                var pagedResult = new PagedList<MedicationResponse>(
                    medicationResponses,
                    medicationsPaged.MetaData.TotalCount,
                    pageNumber,
                    pageSize);

                // Generate success message
                var message = GenerateGetMedicationsSuccessMessage(pagedResult, normalizedSearchTerm, category, includeDeleted);

                // Log success metrics
                stopwatch.Stop();
                _logger.LogInformation(
                    "GetMedicationsAsync completed successfully - User: {UserId}, Duration: {Duration}ms, " +
                    "TotalCount: {TotalCount}, PageCount: {PageCount}, CurrentPage: {CurrentPage}",
                    currentUserId, stopwatch.ElapsedMilliseconds, pagedResult.MetaData.TotalCount,
                    pagedResult.MetaData.TotalPages, pagedResult.MetaData.CurrentPage);

                return ApiResult<PagedList<MedicationResponse>>.Success(pagedResult, message);
            }
            catch (ArgumentException argEx)
            {
                stopwatch.Stop();
                _logger.LogWarning(argEx,
                    "Invalid argument in GetMedicationsAsync by user {UserId} - Duration: {Duration}ms",
                    currentUserId, stopwatch.ElapsedMilliseconds);
                return ApiResult<PagedList<MedicationResponse>>.Failure(argEx);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "Error occurred while getting medications by user {UserId} - Duration: {Duration}ms, " +
                    "Parameters: PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm='{SearchTerm}', " +
                    "Category={Category}, IncludeDeleted={IncludeDeleted}",
                    currentUserId, stopwatch.ElapsedMilliseconds, pageNumber, pageSize,
                    searchTerm, category, includeDeleted);

                return ApiResult<PagedList<MedicationResponse>>.Failure(
                    new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc. Vui lòng thử lại sau."));
            }
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
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = medicationIds.Count
                    };

                    foreach (var id in medicationIds)
                    {
                        try
                        {
                            bool success;

                            if (isPermanent)
                            {
                                success = await _medicationRepository.PermanentDeleteWithLotsAsync(id);
                            }
                            else
                            {
                                success = await _medicationRepository.SoftDeleteWithLotsAsync(id, currentUserId);
                            }

                            if (success)
                            {
                                result.SuccessCount++;
                                result.SuccessIds.Add(id.ToString());
                            }
                            else
                            {
                                result.FailureCount++;
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "NotFound",
                                    Details = "Không tìm thấy thuốc hoặc thuốc đã bị xóa"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            result.FailureCount++;
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = id.ToString(),
                                Error = "Exception",
                                Details = ex.Message
                            });

                            _logger.LogError(ex, "Error deleting medication {MedicationId}", id);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();

                    var actionType = isPermanent ? "xóa vĩnh viễn" : "xóa";
                    result.Message = GenerateBatchOperationMessage(result, actionType);

                    _logger.LogInformation("Medications {ActionType}: Success={SuccessCount}, Failed={FailedCount} by user: {UserId}",
                        actionType, result.SuccessCount, result.FailureCount, currentUserId);

                    return result.SuccessCount > 0
                        ? ApiResult<BatchOperationResultDTO>.Success(result, result.Message)
                        : ApiResult<BatchOperationResultDTO>.Failure(new Exception(result.Message));
                }
                catch (Exception ex)
                {
                    var actionType = isPermanent ? "permanently deleting" : "soft deleting";
                    _logger.LogError(ex, "Error occurred while {ActionType} medications", actionType);
                    return ApiResult<BatchOperationResultDTO>.Failure(new Exception($"Đã xảy ra lỗi khi xóa thuốc"));
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
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = medicationIds.Count
                    };

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
                                result.FailureCount++;
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "NotFound",
                                    Details = "Không tìm thấy thuốc đã bị xóa"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            result.FailureCount++;
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = id.ToString(),
                                Error = "Exception",
                                Details = ex.Message
                            });

                            _logger.LogError(ex, "Error restoring medication {MedicationId}", id);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();

                    result.Message = GenerateBatchOperationMessage(result, "khôi phục");

                    _logger.LogInformation("Medications restored: Success={SuccessCount}, Failed={FailedCount} by user: {UserId}",
                        result.SuccessCount, result.FailureCount, currentUserId);

                    return result.SuccessCount > 0
                        ? ApiResult<BatchOperationResultDTO>.Success(result, result.Message)
                        : ApiResult<BatchOperationResultDTO>.Failure(new Exception(result.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while restoring medications");
                    return ApiResult<BatchOperationResultDTO>.Failure(new Exception("Đã xảy ra lỗi khi khôi phục thuốc"));
                }
            });
        }


        /// <summary>
        /// Lấy thông tin chi tiết thuốc theo Id.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> GetMedicationByIdAsync(Guid id)
        {
            try
            {
                var medication = await _medicationRepository
                    .GetByIdAsync(id, m => m.Lots);

                if (medication == null || medication.IsDeleted)
                {
                    return ApiResult<MedicationResponse>.Failure(new Exception("Không tìm thấy thuốc"));
                }

                var response = await MapToMedicationResponseAsync(medication);
                return ApiResult<MedicationResponse>.Success(response, "Lấy thông tin thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medication by id: {MedicationId}", id);
                return ApiResult<MedicationResponse>.Failure(new Exception("Đã xảy ra lỗi khi lấy thông tin thuốc"));
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
                    // Kiểm tra trùng tên
                    if (await _medicationRepository.MedicationNameExistsAsync(request.Name))
                    {
                        return ApiResult<MedicationResponse>.Failure(new Exception("Tên thuốc đã tồn tại"));
                    }

                    // Tạo entity
                    var medication = MapToMedicationEntity(request);

                    // Sử dụng method từ BaseService để handle audit fields
                    var createdMedication = await CreateAsync(medication);

                    var response = await MapToMedicationResponseAsync(createdMedication);
                    _logger.LogInformation("Medication created successfully: {MedicationId}", createdMedication.Id);

                    return ApiResult<MedicationResponse>.Success(response, "Tạo thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating medication");
                    return ApiResult<MedicationResponse>.Failure(new Exception("Đã xảy ra lỗi khi tạo thuốc"));
                }
            });
        }

        /// <summary>
        /// Cập nhật thông tin thuốc theo Id.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> UpdateMedicationAsync(
            Guid id,
            UpdateMedicationRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var medication = await _medicationRepository
                        .GetByIdAsync(id, m => m.Lots);

                    if (medication == null || medication.IsDeleted)
                    {
                        return ApiResult<MedicationResponse>.Failure(new Exception("Không tìm thấy thuốc"));
                    }

                    // Kiểm tra trùng tên nếu có thay đổi
                    if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != medication.Name)
                    {
                        bool existed = await _medicationRepository
                            .MedicationNameExistsAsync(request.Name, id);

                        if (existed)
                        {
                            return ApiResult<MedicationResponse>.Failure(new Exception("Tên thuốc đã tồn tại"));
                        }
                    }

                    // Update entity properties
                    UpdateMedicationEntity(medication, request);

                    // Sử dụng method từ BaseService để handle audit fields
                    var updatedMedication = await UpdateAsync(medication);

                    var response = await MapToMedicationResponseAsync(updatedMedication);
                    _logger.LogInformation("Medication updated successfully: {MedicationId}", id);

                    return ApiResult<MedicationResponse>.Success(response, "Cập nhật thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating medication: {MedicationId}", id);
                    return ApiResult<MedicationResponse>.Failure(new Exception("Đã xảy ra lỗi khi cập nhật thuốc"));
                }
            });
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
                    var deletedCount = await _medicationRepository.PermanentDeleteExpiredAsync(daysToExpire);

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} expired medications older than {Days} days", deletedCount, daysToExpire);

                    var message = deletedCount > 0
                        ? $"Đã xóa vĩnh viễn {deletedCount} thuốc hết hạn"
                        : "Không có thuốc hết hạn nào để xóa";

                    return ApiResult<string>.Success(message, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired medications");
                    return ApiResult<string>.Failure(new Exception("Đã xảy ra lỗi khi dọn dẹp thuốc hết hạn"));
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
                var medications = await _medicationRepository
                    .GetActiveMedicationsAsync();

                var responses = await MapToMedicationResponsesAsync(medications);

                return ApiResult<List<MedicationResponse>>.Success(responses, "Lấy danh sách thuốc đang hoạt động thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active medications");
                return ApiResult<List<MedicationResponse>>.Failure(new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc đang hoạt động"));
            }
        }

        // Thêm vào MedicationService class
        public async Task<ApiResult<MedicationDetailResponse>> GetMedicationDetailByIdAsync(Guid id)
        {
            try
            {
                // Lấy thông tin thuốc với include lots
                var medication = await _medicationRepository
                    .GetByIdAsync(id, m => m.Lots);

                if (medication == null || medication.IsDeleted)
                {
                    return ApiResult<MedicationDetailResponse>.Failure(new Exception("Không tìm thấy thuốc"));
                }

                // Lấy danh sách lô thuốc chi tiết
                var lots = await _unitOfWork.MedicationLotRepository
                    .GetLotsByMedicationIdAsync(id);

                // Map thông tin thuốc
                var response = new MedicationDetailResponse
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

                    // Map danh sách lô thuốc
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

                return ApiResult<MedicationDetailResponse>.Success(response, "Lấy thông tin chi tiết thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medication detail by id: {MedicationId}", id);
                return ApiResult<MedicationDetailResponse>.Failure(new Exception("Đã xảy ra lỗi khi lấy thông tin chi tiết thuốc"));
            }
        }

        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Map collection of Medication entities to MedicationResponses with optimized performance.
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

                var responses = new List<MedicationResponse>();
                var currentUserId = _currentUserService.GetUserId();

                _logger.LogDebug("Mapping {Count} medications to responses for user {UserId}",
                    medicationList.Count, currentUserId);

                // Process medications in batches for better performance with large datasets
                const int batchSize = 20;
                var totalBatches = (int)Math.Ceiling((double)medicationList.Count / batchSize);

                for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    var batch = medicationList
                        .Skip(batchIndex * batchSize)
                        .Take(batchSize)
                        .ToList();

                    var batchTasks = batch.Select(async medication =>
                    {
                        try
                        {
                            return await MapToMedicationResponseAsync(medication);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error mapping medication {MedicationId} to response", medication.Id);
                            // Return a basic response even if detailed mapping fails
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
                    });

                    var batchResponses = await Task.WhenAll(batchTasks);
                    responses.AddRange(batchResponses);

                    _logger.LogDebug("Completed batch {BatchIndex}/{TotalBatches} for user {UserId}",
                        batchIndex + 1, totalBatches, currentUserId);
                }

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
        /// <summary>
        /// Map CreateMedicationRequest to Medication entity.
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
        /// Update Medication entity với UpdateMedicationRequest.
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

        /// <summary>
        /// Map Medication entity to MedicationResponse.
        /// </summary>
        private async Task<MedicationResponse> MapToMedicationResponseAsync(Medication medication)
        {
            int totalQuantity = await _medicationRepository
                .GetTotalQuantityByMedicationIdAsync(medication.Id);

            return new MedicationResponse
            {
                Id = medication.Id,
                Name = medication.Name,
                Unit = medication.Unit,
                DosageForm = medication.DosageForm,
                Category = medication.Category,
                Status = medication.Status,
                CreatedAt = medication.CreatedAt,
                UpdatedAt = medication.UpdatedAt,
                TotalLots = medication.Lots?.Count ?? 0,
                TotalQuantity = totalQuantity
            };
        }

        /// <summary>
        /// Tạo thông báo thành công cho việc lấy danh sách thuốc.
        /// </summary>
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

            // Thêm thông tin về điều kiện tìm kiếm
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

        /// <summary>
        /// Lấy tên hiển thị cho danh mục thuốc.
        /// </summary>
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
    }
}