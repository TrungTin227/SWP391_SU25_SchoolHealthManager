using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using Microsoft.AspNetCore.Http;

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
                QuantityDelivered = request.QuantityDelivered,
                DeliveredAt = request.DeliveredAt,
                Notes = request.Notes,
                Status = request.Status
            };
        }

        public static ParentMedicationDelivery ToUpdateParentMedicationDelivery(this UpdateParentMedicationDeliveryRequestDTO request, ParentMedicationDelivery existingDelivery)
        {
            if (request.StudentId.HasValue)
                existingDelivery.StudentId = request.StudentId.Value;

            if (request.ParentId.HasValue)
                existingDelivery.ParentId = request.ParentId.Value;

            if (request.ReceivedBy.HasValue)
                existingDelivery.ReceivedBy = request.ReceivedBy.Value;

            if (request.QuantityDelivered.HasValue && request.QuantityDelivered.Value > 0)
                existingDelivery.QuantityDelivered = request.QuantityDelivered.Value;

            if (request.DeliveredAt.HasValue && request.DeliveredAt.Value != default(DateTime))
                existingDelivery.DeliveredAt = request.DeliveredAt.Value;

            if (!string.IsNullOrWhiteSpace(request.Notes))
                existingDelivery.Notes = request.Notes;

            if (request.Status.HasValue && Enum.IsDefined(typeof(StatusMedicationDelivery), request.Status.Value))
                existingDelivery.Status = request.Status.Value;

            return existingDelivery;
        }


    }
}
