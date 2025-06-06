using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class MedicationLotService : IMedicationLotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MedicationLotService> _logger;

        public MedicationLotService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<MedicationLotService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetMedicationLotsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            Guid? medicationId = null,
            bool? isExpired = null)
        {
            try
            {
                var lots = await _unitOfWork.MedicationLotRepository.GetMedicationLotsAsync(
                    pageNumber, pageSize, searchTerm, medicationId, isExpired);

                var lotDTOs = lots.Select(MapToResponseDTO).ToList();

                var result = new PagedList<MedicationLotResponseDTO>(
                    lotDTOs,
                    lots.MetaData.TotalCount,
                    lots.MetaData.CurrentPage,
                    lots.MetaData.PageSize);

                return ApiResult<PagedList<MedicationLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication lots");
                return ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicationLotResponseDTO>> GetMedicationLotByIdAsync(Guid id)
        {
            try
            {
                var lot = await _unitOfWork.MedicationLotRepository.GetLotWithMedicationAsync(id);
                if (lot == null)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không tìm thấy lô thuốc"));
                }

                var lotDTO = MapToResponseDTO(lot);
                return ApiResult<MedicationLotResponseDTO>.Success(
                    lotDTO, "Lấy thông tin lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication lot by ID: {LotId}", id);
                return ApiResult<MedicationLotResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicationLotResponseDTO>> CreateMedicationLotAsync(CreateMedicationLotRequest request)
        {
            try
            {
                // Validate medication exists
                var medication = await _unitOfWork.MedicationRepository.GetByIdAsync(request.MedicationId);
                if (medication == null)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không tìm thấy thuốc"));
                }

                // Check if lot number already exists
                var lotNumberExists = await _unitOfWork.MedicationLotRepository.LotNumberExistsAsync(request.LotNumber);
                if (lotNumberExists)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception($"Số lô '{request.LotNumber}' đã tồn tại"));
                }

                // Validate expiry date
                if (request.ExpiryDate.Date <= DateTime.UtcNow.Date)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Ngày hết hạn phải lớn hơn ngày hiện tại"));
                }

                // FIX: Handle nullable Guid properly - convert to Guid if not null, otherwise use default Guid
                var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                var lot = new MedicationLot
                {
                    MedicationId = request.MedicationId,
                    LotNumber = request.LotNumber,
                    ExpiryDate = request.ExpiryDate,
                    Quantity = request.Quantity,
                    StorageLocation = request.StorageLocation,
                    CreatedBy = currentUserId,
                    UpdatedBy = currentUserId
                };

                await _unitOfWork.MedicationLotRepository.AddAsync(lot);
                await _unitOfWork.SaveChangesAsync();

                // Get the created lot with medication info
                var createdLot = await _unitOfWork.MedicationLotRepository.GetLotWithMedicationAsync(lot.Id);
                if (createdLot == null)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không thể lấy thông tin lô thuốc vừa tạo"));
                }

                var lotDTO = MapToResponseDTO(createdLot);

                return ApiResult<MedicationLotResponseDTO>.Success(
                    lotDTO, "Tạo lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medication lot");
                return ApiResult<MedicationLotResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicationLotResponseDTO>> UpdateMedicationLotAsync(
            Guid id,
            UpdateMedicationLotRequest request)
        {
            try
            {
                var lot = await _unitOfWork.MedicationLotRepository.GetByIdAsync(id);
                if (lot == null)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không tìm thấy lô thuốc"));
                }

                // Check if lot number already exists (excluding current lot)
                var lotNumberExists = await _unitOfWork.MedicationLotRepository.LotNumberExistsAsync(
                    request.LotNumber, id);
                if (lotNumberExists)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception($"Số lô '{request.LotNumber}' đã tồn tại"));
                }

                // Update lot properties
                lot.LotNumber = request.LotNumber;
                lot.ExpiryDate = request.ExpiryDate;
                lot.Quantity = request.Quantity;
                lot.StorageLocation = request.StorageLocation;
                lot.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Empty; // FIX: Handle nullable Guid

                await _unitOfWork.MedicationLotRepository.UpdateAsync(lot);
                await _unitOfWork.SaveChangesAsync();

                // Get updated lot with medication info
                var updatedLot = await _unitOfWork.MedicationLotRepository.GetLotWithMedicationAsync(id);
                if (updatedLot == null)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không thể lấy thông tin lô thuốc đã cập nhật"));
                }

                var lotDTO = MapToResponseDTO(updatedLot);

                return ApiResult<MedicationLotResponseDTO>.Success(
                    lotDTO, "Cập nhật lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medication lot: {LotId}", id);
                return ApiResult<MedicationLotResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> DeleteMedicationLotAsync(Guid id)
        {
            try
            {
                var lot = await _unitOfWork.MedicationLotRepository.GetByIdAsync(id);
                if (lot == null)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Không tìm thấy lô thuốc"));
                }

                await _unitOfWork.MedicationLotRepository.SoftDeleteAsync(lot);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, "Xóa lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medication lot: {LotId}", id);
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<MedicationLotResponseDTO>> RestoreMedicationLotAsync(Guid id)
        {
            try
            {
                // includeDeleted: true để lấy cả bản ghi đã xóa
                var lot = await _unitOfWork.MedicationLotRepository.GetByIdAsync(id, includeDeleted: true);
                if (lot == null || !lot.IsDeleted)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không tìm thấy lô thuốc đã bị xóa"));
                }

                lot.IsDeleted = false;
                lot.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Empty; // FIX: Handle nullable Guid
                lot.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.MedicationLotRepository.UpdateAsync(lot);
                await _unitOfWork.SaveChangesAsync();

                var restoredLot = await _unitOfWork.MedicationLotRepository.GetLotWithMedicationAsync(id);
                if (restoredLot == null)
                {
                    return ApiResult<MedicationLotResponseDTO>.Failure(
                        new Exception("Không thể lấy thông tin lô thuốc đã khôi phục"));
                }

                var lotDTO = MapToResponseDTO(restoredLot);

                return ApiResult<MedicationLotResponseDTO>.Success(
                    lotDTO, "Khôi phục lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring medication lot: {LotId}", id);
                return ApiResult<MedicationLotResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> PermanentDeleteMedicationLotAsync(Guid id)
        {
            try
            {
                var lot = await _unitOfWork.MedicationLotRepository.GetByIdAsync(id, includeDeleted: true);
                if (lot == null)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Không tìm thấy lô thuốc"));
                }

                await _unitOfWork.MedicationLotRepository.DeleteAsync(lot);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, "Xóa vĩnh viễn lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting medication lot: {LotId}", id);
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<PagedList<MedicationLotResponseDTO>>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var lots = await _unitOfWork.MedicationLotRepository.GetSoftDeletedLotsAsync(
                    pageNumber, pageSize, searchTerm);

                var lotDTOs = lots.Select(MapToResponseDTO).ToList();

                var result = new PagedList<MedicationLotResponseDTO>(
                    lotDTOs,
                    lots.MetaData.TotalCount,
                    lots.MetaData.CurrentPage,
                    lots.MetaData.PageSize);

                return ApiResult<PagedList<MedicationLotResponseDTO>>.Success(
                    result, "Lấy danh sách lô thuốc đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted lots");
                return ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiringLotsAsync(int daysBeforeExpiry = 30)
        {
            try
            {
                var lots = await _unitOfWork.MedicationLotRepository.GetExpiringLotsAsync(daysBeforeExpiry);
                var lotDTOs = lots.Select(MapToResponseDTO).ToList();

                return ApiResult<List<MedicationLotResponseDTO>>.Success(
                    lotDTOs, $"Lấy danh sách lô thuốc sắp hết hạn trong {daysBeforeExpiry} ngày thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring lots");
                return ApiResult<List<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationLotResponseDTO>>> GetExpiredLotsAsync()
        {
            try
            {
                var lots = await _unitOfWork.MedicationLotRepository.GetExpiredLotsAsync();
                var lotDTOs = lots.Select(MapToResponseDTO).ToList();

                return ApiResult<List<MedicationLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô thuốc đã hết hạn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired lots");
                return ApiResult<List<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<MedicationLotResponseDTO>>> GetLotsByMedicationIdAsync(Guid medicationId)
        {
            try
            {
                var lots = await _unitOfWork.MedicationLotRepository.GetLotsByMedicationIdAsync(medicationId);
                var lotDTOs = lots.Select(MapToResponseDTO).ToList();

                return ApiResult<List<MedicationLotResponseDTO>>.Success(
                    lotDTOs, "Lấy danh sách lô thuốc theo ID thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lots by medication ID: {MedicationId}", medicationId);
                return ApiResult<List<MedicationLotResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<int>> GetAvailableQuantityAsync(Guid medicationId)
        {
            try
            {
                var quantity = await _unitOfWork.MedicationLotRepository.GetAvailableQuantityAsync(medicationId);
                return ApiResult<int>.Success(quantity, "Lấy số lượng khả dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available quantity for medication: {MedicationId}", medicationId);
                return ApiResult<int>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> UpdateQuantityAsync(Guid lotId, int newQuantity)
        {
            try
            {
                if (newQuantity < 0)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Số lượng không được âm"));
                }

                var success = await _unitOfWork.MedicationLotRepository.UpdateQuantityAsync(lotId, newQuantity);
                if (!success)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Không tìm thấy lô thuốc"));
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResult<bool>.Success(true, "Cập nhật số lượng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for lot: {LotId}", lotId);
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<int>> CleanupExpiredLotsAsync(int daysToExpire = 90)
        {
            try
            {
                var expiredDate = DateTime.UtcNow.Date.AddDays(-daysToExpire);
                var expiredLots = await _unitOfWork.MedicationLotRepository.GetAllAsync();

                var lotsToDelete = expiredLots
                    .Where(lot => lot.IsDeleted && lot.UpdatedAt.Date <= expiredDate)
                    .ToList();

                foreach (var lot in lotsToDelete)
                {
                    await _unitOfWork.MedicationLotRepository.DeleteAsync(lot);
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<int>.Success(
                    lotsToDelete.Count,
                    $"Đã dọn dẹp {lotsToDelete.Count} lô thuốc hết hạn");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired lots");
                return ApiResult<int>.Failure(ex);
            }
        }
        /// <summary>
        /// Get comprehensive statistics for medication lots - SINGLE QUERY VERSION
        /// </summary>
        public async Task<ApiResult<MedicationLotStatisticsResponseDTO>> GetStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to calculate medication lot statistics");

                // Use existing repository methods to gather statistics
                var totalLots = await _unitOfWork.MedicationLotRepository.GetTotalLotCountAsync();
                var activeLots = await _unitOfWork.MedicationLotRepository.GetActiveLotCountAsync();
                var expiredLots = await _unitOfWork.MedicationLotRepository.GetExpiredLotCountAsync();
                var expiringLots = await _unitOfWork.MedicationLotRepository.GetExpiringLotCountAsync(30);

                var statistics = new MedicationLotStatisticsResponseDTO
                {
                    TotalLots = totalLots,
                    ActiveLots = activeLots,
                    ExpiredLots = expiredLots,
                    ExpiringInNext30Days = expiringLots,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Successfully calculated medication lot statistics: Total={TotalLots}, Active={ActiveLots}, Expired={ExpiredLots}, Expiring={ExpiringLots}",
                    statistics.TotalLots, statistics.ActiveLots, statistics.ExpiredLots, statistics.ExpiringInNext30Days);

                return ApiResult<MedicationLotStatisticsResponseDTO>.Success(
                    statistics, "Lấy thống kê lô thuốc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating medication lot statistics");
                return ApiResult<MedicationLotStatisticsResponseDTO>.Failure(ex);
            }
        }

        // Manual mapping methods
        private static MedicationLotResponseDTO MapToResponseDTO(MedicationLot lot)
        {
            return new MedicationLotResponseDTO
            {
                Id = lot.Id,
                MedicationId = lot.MedicationId,
                LotNumber = lot.LotNumber,
                ExpiryDate = lot.ExpiryDate,
                Quantity = lot.Quantity,
                StorageLocation = lot.StorageLocation,
                IsDeleted = lot.IsDeleted,
                CreatedAt = lot.CreatedAt,
                UpdatedAt = lot.UpdatedAt,
                CreatedBy = lot.CreatedBy.ToString(), // Handle nullable Guid for display
                UpdatedBy = lot.UpdatedBy.ToString(), // Handle nullable Guid for display
                MedicationName = lot.Medication?.Name ?? string.Empty,
                MedicationUnit = lot.Medication?.Unit ?? string.Empty
            };
        }
    }
}