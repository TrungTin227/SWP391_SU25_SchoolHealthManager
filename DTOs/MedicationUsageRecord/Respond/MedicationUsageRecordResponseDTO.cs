using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MedicationUsageRecord.Respond
{
    public class MedicationUsageRecordResponseDTO
    {
        public Guid Id { get; set; }
        public Guid DeliveryDetailId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public int totalQuantity { get; set; }
        public int QuantityUsed { get; set; }
        public int QuantityRemaining { get; set; }
        public string dosageInstruction { get; set; } = string.Empty;
        public DateTime UsedAt { get; set; }
        public bool IsTaken { get; set; }
        public string? Note { get; set; }
        public Guid StudentId { get; set; }         // Add this
        public string? StudentName { get; set; }    // Add this
    }

}
