using BusinessObjects.Common;
using DTOs.ParentMedicationDeliveryDetail.Respond;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentMedicationDeliveryDTOs.Respond
{
    public class ParentMedicationDeliveryResponseDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid ParentId { get; set; }
        public Guid ReceivedBy { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime DeliveredAt { get; set; }
        public StatusMedicationDelivery Status { get; set; }
        public List<ParentMedicationDeliveryDetailResponseDTO> Medications { get; set; } = new();
    }

}
