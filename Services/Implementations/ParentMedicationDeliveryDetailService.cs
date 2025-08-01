using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interfaces;
using Services.Commons;
using Services.Interfaces;

namespace Services.Implementations
{
    public class ParentMedicationDeliveryDetailService : BaseService<ParentMedicationDeliveryDetail, Guid>, IParentMedicationDeliveryDetailService
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ILogger<ParentMedicationDeliveryDetailService> _logger;

        public ParentMedicationDeliveryDetailService(
            IGenericRepository<ParentMedicationDeliveryDetail, Guid> repository, 
            ICurrentUserService currentUserService, 
            IUnitOfWork unitOfWork, 
            ICurrentTime currentTime,
            SchoolHealthManagerDbContext dbContext,
            ILogger<ParentMedicationDeliveryDetailService> logger) 
            : base(repository, currentUserService, unitOfWork, currentTime)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Cập nhật ReturnedQuantity khi kết thúc đợt phát thuốc
        /// </summary>
        public async Task<ApiResult<bool>> UpdateReturnedQuantityAsync(Guid deliveryDetailId)
        {
            try
            {
                _logger.LogInformation("Cập nhật ReturnedQuantity cho delivery detail. DeliveryDetailId: {DeliveryDetailId}", deliveryDetailId);

                if (deliveryDetailId == Guid.Empty)
                {
                    _logger.LogError("DeliveryDetailId không hợp lệ");
                    return ApiResult<bool>.Failure(new ArgumentException("DeliveryDetailId không hợp lệ"));
                }

                var deliveryDetail = await _dbContext.ParentMedicationDeliveryDetails
                    .Include(d => d.UsageRecords)
                    .FirstOrDefaultAsync(d => d.Id == deliveryDetailId);

                if (deliveryDetail == null)
                {
                    _logger.LogWarning("Không tìm thấy delivery detail với ID: {DeliveryDetailId}", deliveryDetailId);
                    return ApiResult<bool>.Failure(new ArgumentException("Không tìm thấy delivery detail"));
                }

                // Sử dụng QuantityUsed đã được tính toán từ MedicationUsageRecordService
                var totalUsed = deliveryDetail.QuantityUsed;

                // Tính toán số lượng thuốc còn lại (thừa)
                var returnedQuantity = Math.Max(0, deliveryDetail.TotalQuantity - totalUsed);

                // Cập nhật ReturnedQuantity và ReturnedAt
                deliveryDetail.ReturnedQuantity = returnedQuantity;
                deliveryDetail.ReturnedAt = _currentTime.GetVietnamTime();

                await _unitOfWork.ParentMedicationDeliveryDetailRepository.UpdateAsync(deliveryDetail);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật ReturnedQuantity thành công. DeliveryDetailId: {DeliveryDetailId}, TotalQuantity: {TotalQuantity}, QuantityUsed: {QuantityUsed}, ReturnedQuantity: {ReturnedQuantity}", 
                    deliveryDetailId, deliveryDetail.TotalQuantity, totalUsed, returnedQuantity);

                return ApiResult<bool>.Success(true, "Cập nhật ReturnedQuantity thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ReturnedQuantity. DeliveryDetailId: {DeliveryDetailId}", deliveryDetailId);
                return ApiResult<bool>.Failure(ex);
            }
        }

        /// <summary>
        /// Cập nhật ReturnedQuantity cho tất cả delivery details của một delivery
        /// </summary>
        public async Task<ApiResult<bool>> UpdateReturnedQuantityForDeliveryAsync(Guid deliveryId)
        {
            try
            {
                _logger.LogInformation("Cập nhật ReturnedQuantity cho tất cả delivery details của delivery. DeliveryId: {DeliveryId}", deliveryId);

                if (deliveryId == Guid.Empty)
                {
                    _logger.LogError("DeliveryId không hợp lệ");
                    return ApiResult<bool>.Failure(new ArgumentException("DeliveryId không hợp lệ"));
                }

                var deliveryDetails = await _dbContext.ParentMedicationDeliveryDetails
                    .Include(d => d.UsageRecords)
                        .ThenInclude(r => r.MedicationSchedule)
                    .Where(d => d.ParentMedicationDeliveryId == deliveryId)
                    .ToListAsync();

                if (!deliveryDetails.Any())
                {
                    _logger.LogWarning("Không tìm thấy delivery details cho delivery ID: {DeliveryId}", deliveryId);
                    return ApiResult<bool>.Failure(new ArgumentException("Không tìm thấy delivery details"));
                }

                var currentTime = _currentTime.GetVietnamTime();
                var updatedCount = 0;

                foreach (var deliveryDetail in deliveryDetails)
                {
                    // Sử dụng QuantityUsed đã được tính toán từ MedicationUsageRecordService
                    var totalUsed = deliveryDetail.QuantityUsed;

                    // Tính toán số lượng thuốc còn lại (thừa)
                    var returnedQuantity = Math.Max(0, deliveryDetail.TotalQuantity - totalUsed);

                    // Cập nhật ReturnedQuantity và ReturnedAt
                    deliveryDetail.ReturnedQuantity = returnedQuantity;
                    deliveryDetail.ReturnedAt = currentTime;

                    await _unitOfWork.ParentMedicationDeliveryDetailRepository.UpdateAsync(deliveryDetail);
                    updatedCount++;

                    _logger.LogInformation("Đã cập nhật ReturnedQuantity cho delivery detail. DeliveryDetailId: {DeliveryDetailId}, TotalQuantity: {TotalQuantity}, QuantityUsed: {QuantityUsed}, ReturnedQuantity: {ReturnedQuantity}", 
                        deliveryDetail.Id, deliveryDetail.TotalQuantity, totalUsed, returnedQuantity);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật ReturnedQuantity cho {UpdatedCount} delivery details của delivery {DeliveryId}", updatedCount, deliveryId);

                return ApiResult<bool>.Success(true, $"Đã cập nhật ReturnedQuantity cho {updatedCount} delivery details!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ReturnedQuantity cho delivery. DeliveryId: {DeliveryId}", deliveryId);
                return ApiResult<bool>.Failure(ex);
            }
        }
    }
}
