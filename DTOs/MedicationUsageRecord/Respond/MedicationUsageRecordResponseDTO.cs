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
        public DateTime UsedAt { get; set; }
        public bool IsTaken { get; set; }
        public string? Note { get; set; }
    }

}
