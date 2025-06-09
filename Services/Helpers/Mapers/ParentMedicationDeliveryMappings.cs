using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Request;

namespace Services.Helpers.Mapers
{
    public static class ParentMedicationDeliveryMappings
    {
        public static ParentMedicationDelivery ToParentMedicationDelivery(this CreateParentMedicationDeliveryRequestDTO request)
        {
            return new ParentMedicationDelivery
            {
                StudentId = request.StudentId,
                ParentId = request.ParentId,
                ReceivedBy = request.ReceivedBy,
                QuantityDelivered = request.QuantityDelivered,
                DeliveredAt = request.DeliveredAt,
                Notes = request.Notes,
                Status = request.Status
            };
        }
    }
}
