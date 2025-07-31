using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentMedicationDeliveryDetail.Request
{
    public class CreateParentMedicationDeliveryDetailDTO
    {
        public string MedicationName { get; set; } = string.Empty;
        public int QuantityDelivered { get; set; }
        public string DosageInstruction { get; set; } = string.Empty;
    }

}
