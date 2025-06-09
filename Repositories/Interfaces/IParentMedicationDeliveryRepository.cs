using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Respond;

namespace Repositories.Interfaces
{
    public interface IParentMedicationDeliveryRepository : IGenericRepository<ParentMedicationDelivery,Guid>
    {
        Task<List<GetParentMedicationDeliveryRespondDTO>> GetAllParentMedicationDeliveryDTO();
        Task<List<GetParentMedicationDeliveryRespondDTO>> GetAllParentMedicationDeliveryByParentIdDTO(Guid id);

        Task<GetParentMedicationDeliveryRespondDTO?> GetParentMedicationDeliveryByIdDTO(Guid id);


    }
}
