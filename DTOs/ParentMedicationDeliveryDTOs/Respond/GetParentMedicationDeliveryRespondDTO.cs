using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;

namespace DTOs.ParentMedicationDeliveryDTOs.Respond
{
    public class GetParentMedicationDeliveryRespondDTO
    {
        public Guid ParentMedicationDeliveryId { get; set; }
        public Guid StudentId { get; set; }

        public Guid ParentId { get; set; }

        public Guid ReceivedBy { get; set; }

        public int QuantityDelivered { get; set; }
        public DateTime DeliveredAt { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
    }
}
