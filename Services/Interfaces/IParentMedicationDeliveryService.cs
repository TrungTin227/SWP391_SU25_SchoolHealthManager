using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Respond;

namespace Services.Interfaces
{
    public interface IParentMedicationDeliveryService
    {
        Task<ApiResult<CreateParentMedicationDeliveryRequestDTO>> CreateAsync(CreateParentMedicationDeliveryRequestDTO request);
        Task<ApiResult<UpdateParentMedicationDeliveryRequestDTO>> UpdateAsync(UpdateParentMedicationDeliveryRequestDTO request);
        //Task<bool> DeleteAsync(Guid id);

        // Thêm method riêng nếu muốn
        //Task<List<ParentMedicationDelivery>> GetByParentIdAsync(Guid parentId);
        Task<ApiResult<List<GetParentMedicationDeliveryRespondDTO>>> GetAllParentMedicationDeliveryByParentIdAsync(Guid parentId);
        Task<ApiResult<GetParentMedicationDeliveryRespondDTO?>> GetByIdAsync(Guid id);
        Task<ApiResult<List<GetParentMedicationDeliveryRespondDTO>>> GetAllAsync();
        Task<ApiResult<List<GetParentMedicationDeliveryRespondDTO>>> GetAllPendingAsync();
        Task<ApiResult<bool>> UpdateStatus(Guid parentMedicationDeliveryid, StatusMedicationDelivery status);
    }
}
