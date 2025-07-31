using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class ParentMedicationDeliveryDetail : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid ParentMedicationDeliveryId { get; set; }
        public ParentMedicationDelivery ParentMedicationDelivery { get; set; } = default!;

        public string MedicationName { get; set; }
        public int TotalQuantity { get; set; }
        public string DosageInstruction { get; set; } // ví dụ: "2 viên sáng, 1 viên chiều"

        public int? ReturnedQuantity { get; set; }
        public DateTime? ReturnedAt { get; set; }

        // Quan hệ
        public List<MedicationUsageRecord> UsageRecords { get; set; } = new();
    }

}
