using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MedicationUsageRecord.Request
{
    public class UpdateMedicationUsageRecordDTO
    {
        public Guid Id { get; set; }
        public bool IsTaken { get; set; }
        public DateTime? TakenAt { get; set; }
        public string? Note { get; set; }
    }

}
