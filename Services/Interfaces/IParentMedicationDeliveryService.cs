using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Request;

namespace Services.Interfaces
{
    public interface IParentMedicationDeliveryService
    {
        Task<ApiResult<CreateParentMedicationDeliveryRequestDTO>> CreateAsync(CreateParentMedicationDeliveryRequestDTO request);
        Task<ApiResult<CreateParentMedicationDeliveryRequestDTO>> UpdateAsync(CreateParentMedicationDeliveryRequestDTO request);
        //Task<bool> DeleteAsync(Guid id);

        // Thêm method riêng nếu muốn
        //Task<List<ParentMedicationDelivery>> GetByParentIdAsync(Guid parentId);
    }
}
