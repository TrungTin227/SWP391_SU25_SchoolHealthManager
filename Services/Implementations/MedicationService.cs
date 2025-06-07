using DTOs.MedicationDTOs.Request;
using DTOs.MedicationDTOs.Response;
using DTOs.MedicationLotDTOs.Response;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Extensions;

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
            ILogger<MedicationService> logger)
            : base(medicationRepository, currentUserService, unitOfWork)
        {
            _medicationRepository = medicationRepository;
            _logger = logger;
        }

        #region Public API Methods

        /// <summary>
        /// Lấy danh sách thuốc theo phân trang, có thể lọc theo searchTerm và category.
        /// </summary>
        public async Task<ApiResult<PagedList<MedicationResponse>>> GetMedicationsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            MedicationCategory? category = null)
        {
            try
            {
                var medicationsPaged = await _medicationRepository
                    .GetMedicationsAsync(pageNumber, pageSize, searchTerm, category);

                var medicationResponses = await MapToMedicationResponsesAsync(medicationsPaged);

                var pagedResult = new PagedList<MedicationResponse>(
                    medicationResponses,
                    medicationsPaged.MetaData.TotalCount,
                    pageNumber,
                    pageSize);

                return ApiResult<PagedList<MedicationResponse>>.Success(pagedResult, "Lấy danh sách thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medications");
                return ApiResult<PagedList<MedicationResponse>>.Failure(new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc"));
            }
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
        /// Xóa mềm thuốc và các lots liên quan.
        /// </summary>
        public async Task<ApiResult<string>> DeleteMedicationAsync(Guid id)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;

                    // Sử dụng method từ repository để soft delete cả lots
                    var success = await _medicationRepository.SoftDeleteWithLotsAsync(id, currentUserId);

                    if (!success)
                    {
                        return ApiResult<string>.Failure(new Exception("Không tìm thấy thuốc hoặc thuốc đã bị xóa"));
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Medication soft deleted successfully: {MedicationId} by user: {UserId}", id, currentUserId);
                    return ApiResult<string>.Success("Xóa thuốc thành công", "Xóa thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while soft deleting medication: {MedicationId}", id);
                    return ApiResult<string>.Failure(new Exception("Đã xảy ra lỗi khi xóa thuốc"));
                }
            });
        }

        /// <summary>
        /// Khôi phục thuốc đã bị soft delete.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> RestoreMedicationAsync(Guid id)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;

                    var success = await _medicationRepository.RestoreWithLotsAsync(id, currentUserId);

                    if (!success)
                    {
                        return ApiResult<MedicationResponse>.Failure(new Exception("Không tìm thấy thuốc đã bị xóa"));
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Lấy lại thông tin medication sau khi restore
                    var medication = await _medicationRepository.GetByIdAsync(id, m => m.Lots);
                    var response = await MapToMedicationResponseAsync(medication);

                    _logger.LogInformation("Medication restored successfully: {MedicationId} by user: {UserId}", id, currentUserId);
                    return ApiResult<MedicationResponse>.Success(response, "Khôi phục thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while restoring medication: {MedicationId}", id);
                    return ApiResult<MedicationResponse>.Failure(new Exception("Đã xảy ra lỗi khi khôi phục thuốc"));
                }
            });
        }

        /// <summary>
        /// Xóa vĩnh viễn thuốc và tất cả lots liên quan.
        /// </summary>
        public async Task<ApiResult<string>> PermanentDeleteMedicationAsync(Guid id)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var success = await _medicationRepository.PermanentDeleteWithLotsAsync(id);

                    if (!success)
                    {
                        return ApiResult<string>.Failure(new Exception("Không tìm thấy thuốc"));
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Medication permanently deleted: {MedicationId}", id);
                    return ApiResult<string>.Success("Xóa vĩnh viễn thuốc thành công", "Xóa vĩnh viễn thuốc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while permanently deleting medication: {MedicationId}", id);
                    return ApiResult<string>.Failure(new Exception("Đã xảy ra lỗi khi xóa vĩnh viễn thuốc"));
                }
            });
        }

        /// <summary>
        /// Lấy danh sách các thuốc đã bị soft delete với phân trang.
        /// </summary>
        public async Task<ApiResult<PagedList<MedicationResponse>>> GetSoftDeletedMedicationsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null)
        {
            try
            {
                var deletedMedicationsPaged = await _medicationRepository
                    .GetSoftDeletedAsync(pageNumber, pageSize, searchTerm);

                var medicationResponses = await MapToMedicationResponsesAsync(deletedMedicationsPaged);

                var pagedResult = new PagedList<MedicationResponse>(
                    medicationResponses,
                    deletedMedicationsPaged.MetaData.TotalCount,
                    pageNumber,
                    pageSize);

                return ApiResult<PagedList<MedicationResponse>>.Success(pagedResult, "Lấy danh sách thuốc đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting soft deleted medications");
                return ApiResult<PagedList<MedicationResponse>>.Failure(new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc đã xóa"));
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
        /// Lấy danh sách thuốc theo category.
        /// </summary>
        public async Task<ApiResult<List<MedicationResponse>>> GetMedicationsByCategoryAsync(MedicationCategory category)
        {
            try
            {
                var medications = await _medicationRepository
                    .GetMedicationsByCategoryAsync(category);

                var responses = await MapToMedicationResponsesAsync(medications);

                return ApiResult<List<MedicationResponse>>.Success(responses, "Lấy danh sách thuốc theo danh mục thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medications by category: {Category}", category);
                return ApiResult<List<MedicationResponse>>.Failure(new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc theo danh mục"));
            }
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
        /// Map collection of Medication entities to MedicationResponses.
        /// </summary>
        private async Task<List<MedicationResponse>> MapToMedicationResponsesAsync(IEnumerable<Medication> medications)
        {
            var responses = new List<MedicationResponse>();
            foreach (var medication in medications)
            {
                var response = await MapToMedicationResponseAsync(medication);
                responses.Add(response);
            }
            return responses;
        }

        #endregion
    }
}