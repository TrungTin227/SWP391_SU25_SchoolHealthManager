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
        public int QuantityUsed { get; set; } = 0; // Số lượng thuốc đã sử dụng
        public int QuantityRemaining { get; set; } // Số lượng thuốc còn lại
        public string? DosageInstruction { get; set; } // Hướng dẫn chung về cách uống thuốc

        public int? ReturnedQuantity { get; set; }
        public DateTime? ReturnedAt { get; set; }

        // Quan hệ
        public List<MedicationSchedule> MedicationSchedules { get; set; } = new();
        public List<MedicationUsageRecord> UsageRecords { get; set; } = new();
    }

}
