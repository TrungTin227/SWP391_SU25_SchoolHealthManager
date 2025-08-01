using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;

namespace Services.Interfaces
{
    public interface IParentMedicationDeliveryDetailService
    {
        /// <summary>
        /// Cập nhật ReturnedQuantity khi kết thúc đợt phát thuốc
        /// </summary>
        Task<ApiResult<bool>> UpdateReturnedQuantityAsync(Guid deliveryDetailId);

        /// <summary>
        /// Cập nhật ReturnedQuantity cho tất cả delivery details của một delivery
        /// </summary>
        Task<ApiResult<bool>> UpdateReturnedQuantityForDeliveryAsync(Guid deliveryId);
    }
}
