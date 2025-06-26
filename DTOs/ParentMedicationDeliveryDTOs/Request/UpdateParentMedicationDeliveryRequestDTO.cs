using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;

namespace DTOs.ParentMedicationDeliveryDTOs.Request
{
    public class UpdateParentMedicationDeliveryRequestDTO
    {
        [Required(ErrorMessage = "ParentMedicationDeliveryId is required.")]
        public Guid ParentMedicationDeliveryId { get; set; }
        public string? MedicationName { get; set; } // Optional, can be null if not updated
        public Guid? StudentId { get; set; }

        public Guid? ParentId { get; set; }

        public Guid? ReceivedBy { get; set; }

        public int? QuantityDelivered { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? Notes { get; set; }
        public StatusMedicationDelivery? Status { get; set; }
    }
}
