using DTOs.MedicationDTOs.Request;
using DTOs.MedicationDTOs.Response;
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class MedicationService : IMedicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MedicationService> _logger;

        public MedicationService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<MedicationService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

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
                // 1) Gọi repository trả về PagedList<Medication>
                var medicationsPaged = await _unitOfWork.MedicationRepository
                    .GetMedicationsAsync(pageNumber, pageSize, searchTerm, category);

                // 2) Map từ Medication thành MedicationResponse kèm tổng số lượng
                var medicationResponses = new List<MedicationResponse>();
                foreach (var medication in medicationsPaged)
                {
                    int totalQuantity = await _unitOfWork.MedicationRepository
                        .GetTotalQuantityByMedicationIdAsync(medication.Id);

                    medicationResponses.Add(MapToMedicationResponse(medication, totalQuantity));
                }

                // 3) Tạo ra PagedList<MedicationResponse> dựa trên metadata của medicationsPaged
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
        /// Lấy thông tin chi tiết thuốc theo Id. Bao gồm cả các lot (để tính tổng lượng).
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> GetMedicationByIdAsync(Guid id)
        {
            try
            {
                // 1) Lấy medication kèm navigation property Lots
                var medication = await _unitOfWork.MedicationRepository
                    .GetByIdAsync(id, m => m.Lots);

                // 2) Nếu không tồn tại hoặc đã bị xóa (IsDeleted == true), trả về lỗi
                if (medication == null || medication.IsDeleted)
                {
                    return ApiResult<MedicationResponse>.Failure(new Exception("Không tìm thấy thuốc"));
                }

                // 3) Tính tổng số lượng hiện có
                int totalQuantity = await _unitOfWork.MedicationRepository
                    .GetTotalQuantityByMedicationIdAsync(medication.Id);

                // 4) Map result
                var response = MapToMedicationResponse(medication, totalQuantity);
                return ApiResult<MedicationResponse>.Success(response, "Lấy thông tin thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medication by id: {MedicationId}", id);
                return ApiResult<MedicationResponse>.Failure(new Exception("Đã xảy ra lỗi khi lấy thông tin thuốc"));
            }
        }

        /// <summary>
        /// Tạo mới một thuốc. Kiểm tra trùng tên trước khi thêm.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> CreateMedicationAsync(CreateMedicationRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // 1) Kiểm tra trùng tên (không bao gồm Id vì là tạo mới)
                    if (await _unitOfWork.MedicationRepository.MedicationNameExistsAsync(request.Name))
                    {
                        return ApiResult<MedicationResponse>.Failure(new Exception("Tên thuốc đã tồn tại"));
                    }

                    // 2) Khởi tạo đối tượng Medication
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var medication = new Medication
                    {
                        Id = Guid.NewGuid(),
                        Name = request.Name,
                        Unit = request.Unit,
                        DosageForm = request.DosageForm,
                        Category = request.Category,
                        Status = request.Status,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId,
                        UpdatedBy = currentUserId,
                        IsDeleted = false
                    };

                    // 3) Thêm mới vào repository và SaveChanges
                    var createdMedication = await _unitOfWork.MedicationRepository.AddAsync(medication);
                    await _unitOfWork.SaveChangesAsync();

                    // 4) Map ra response (tổng lượng khi mới tạo = 0)
                    var response = MapToMedicationResponse(createdMedication, 0);
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
        /// Cập nhật thông tin thuốc theo Id. Có thể thay đổi tên, đơn vị, dạng bào chế, category, status.
        /// </summary>
        public async Task<ApiResult<MedicationResponse>> UpdateMedicationAsync(
            Guid id,
            UpdateMedicationRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // 1) Lấy về đối tượng Medication (kèm Lots nếu cần tính lại totalQuantity sau)
                    var medication = await _unitOfWork.MedicationRepository
                        .GetByIdAsync(id, m => m.Lots);

                    if (medication == null || medication.IsDeleted)
                    {
                        return ApiResult<MedicationResponse>.Failure(new Exception("Không tìm thấy thuốc"));
                    }

                    // 2) Nếu thay đổi tên, kiểm tra trùng tên (ngoại trừ chính nó)
                    if (!string.IsNullOrWhiteSpace(request.Name))
                    {
                        bool existed = await _unitOfWork.MedicationRepository
                            .MedicationNameExistsAsync(request.Name, id);

                        if (existed)
                        {
                            return ApiResult<MedicationResponse>.Failure(new Exception("Tên thuốc đã tồn tại"));
                        }
                        medication.Name = request.Name;
                    }

                    // 3) Cập nhật các thuộc tính khác nếu có
                    if (!string.IsNullOrWhiteSpace(request.Unit))
                        medication.Unit = request.Unit;

                    if (!string.IsNullOrWhiteSpace(request.DosageForm))
                        medication.DosageForm = request.DosageForm;

                    if (request.Category.HasValue)
                        medication.Category = request.Category.Value;

                    if (request.Status.HasValue)
                        medication.Status = request.Status.Value;

                    // 4) Cập nhật UpdatedBy / UpdatedAt
                    medication.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Empty;
                    medication.UpdatedAt = DateTime.UtcNow;

                    // 5) Gọi Update và SaveChanges
                    await _unitOfWork.MedicationRepository.UpdateAsync(medication);
                    await _unitOfWork.SaveChangesAsync();

                    // 6) Tính lại tổng quantity và map
                    int totalQuantity = await _unitOfWork.MedicationRepository
                        .GetTotalQuantityByMedicationIdAsync(medication.Id);

                    var response = MapToMedicationResponse(medication, totalQuantity);
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
        /// Xóa mềm (soft delete) một thuốc theo Id - Sử dụng method mới của repository với user tracking.
        /// </summary>
        public async Task<ApiResult<string>> DeleteMedicationAsync(Guid id)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;

                    // Sử dụng method SoftDeleteAsync mới của repository
                    var success = await _unitOfWork.MedicationRepository.SoftDeleteAsync(id, currentUserId);

                    if (!success)
                    {
                        return ApiResult<string>.Failure(new Exception("Không tìm thấy thuốc hoặc thuốc đã bị xóa"));
                    }

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

                    var success = await _unitOfWork.MedicationRepository.RestoreAsync(id, currentUserId);

                    if (!success)
                    {
                        return ApiResult<MedicationResponse>.Failure(new Exception("Không tìm thấy thuốc đã bị xóa"));
                    }

                    // Lấy lại thông tin medication sau khi restore
                    var medication = await _unitOfWork.MedicationRepository.GetByIdAsync(id, m => m.Lots);
                    var totalQuantity = await _unitOfWork.MedicationRepository.GetTotalQuantityByMedicationIdAsync(id);

                    var response = MapToMedicationResponse(medication, totalQuantity);

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
                    var success = await _unitOfWork.MedicationRepository.PermanentDeleteAsync(id);

                    if (!success)
                    {
                        return ApiResult<string>.Failure(new Exception("Không tìm thấy thuốc"));
                    }

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
                
                var deletedMedicationsPaged = await _unitOfWork.MedicationRepository
                    .GetSoftDeletedAsync(pageNumber, pageSize, searchTerm);

                var medicationResponses = new List<MedicationResponse>();
                foreach (var medication in deletedMedicationsPaged)
                {
                    // Tính tổng quantity cho cả những thuốc đã bị xóa
                    int totalQuantity = await _unitOfWork.MedicationRepository
                        .GetTotalQuantityByMedicationIdAsync(medication.Id);

                    medicationResponses.Add(MapToMedicationResponse(medication, totalQuantity));
                }

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
                    var deletedCount = await _unitOfWork.MedicationRepository.PermanentDeleteExpiredAsync(daysToExpire);

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
        /// Lấy danh sách thuốc theo một category cố định (không phân trang).
        /// </summary>
        public async Task<ApiResult<List<MedicationResponse>>> GetMedicationsByCategoryAsync(MedicationCategory category)
        {
            try
            {
                // 1) Lấy về danh sách Medication (loại bỏ IsDeleted) theo category
                var medications = await _unitOfWork.MedicationRepository
                    .GetMedicationsByCategoryAsync(category);

                // 2) Map từng phần tử và lấy tổng quantity
                var responses = new List<MedicationResponse>();
                foreach (var medication in medications)
                {
                    int totalQuantity = await _unitOfWork.MedicationRepository
                        .GetTotalQuantityByMedicationIdAsync(medication.Id);

                    responses.Add(MapToMedicationResponse(medication, totalQuantity));
                }

                return ApiResult<List<MedicationResponse>>.Success(responses, "Lấy danh sách thuốc theo danh mục thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting medications by category: {Category}", category);
                return ApiResult<List<MedicationResponse>>.Failure(new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc theo danh mục"));
            }
        }

        /// <summary>
        /// Lấy danh sách thuốc đang ở trạng thái Active (không phân trang).
        /// </summary>
        public async Task<ApiResult<List<MedicationResponse>>> GetActiveMedicationsAsync()
        {
            try
            {
                var medications = await _unitOfWork.MedicationRepository
                    .GetActiveMedicationsAsync();

                var responses = new List<MedicationResponse>();
                foreach (var medication in medications)
                {
                    int totalQuantity = await _unitOfWork.MedicationRepository
                        .GetTotalQuantityByMedicationIdAsync(medication.Id);

                    responses.Add(MapToMedicationResponse(medication, totalQuantity));
                }

                return ApiResult<List<MedicationResponse>>.Success(responses, "Lấy danh sách thuốc đang hoạt động thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active medications");
                return ApiResult<List<MedicationResponse>>.Failure(new Exception("Đã xảy ra lỗi khi lấy danh sách thuốc đang hoạt động"));
            }
        }

        /// <summary>
        /// Hàm helper để map entity Medication + tổng quantity thành MedicationResponse.
        /// </summary>
        private static MedicationResponse MapToMedicationResponse(Medication medication, int totalQuantity)
        {
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
    }
}