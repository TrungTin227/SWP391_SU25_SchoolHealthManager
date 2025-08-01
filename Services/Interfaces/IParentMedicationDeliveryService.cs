using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Respond;

namespace Services.Interfaces
{
    public interface IParentMedicationDeliveryService
    {
        Task<ApiResult<ParentMedicationDeliveryResponseDTO>> CreateDeliveryAsync(CreateParentMedicationDeliveryRequestDTO request);
        Task<ApiResult<ParentMedicationDeliveryResponseDTO>> GetByIdAsync(Guid id);
        Task<ApiResult<List<ParentMedicationDeliveryResponseDTO>>> GetByStudentIdAsync(Guid studentId);
        Task<ApiResult<List<ParentMedicationDeliveryResponseDTO>>> GetAllAsync();
        Task<ApiResult<List<ParentMedicationDeliveryResponseDTO>>> GetAllForCurrentParentAsync();
        Task<ApiResult<ParentMedicationDeliveryResponseDTO>> UpdateStatusAsync(Guid deliveryId, StatusMedicationDelivery status);
        Task<ApiResult<ParentMedicationDeliveryResponseDTO>> DeleteAsync(Guid deliveryId);
    }
}
