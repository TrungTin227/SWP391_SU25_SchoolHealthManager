using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentMedicationDeliveryDetail.Respond
{
    public class ParentMedicationDeliveryDetailResponseDTO
    {
        public Guid Id { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int QuantityUsed { get; set; }
        public int QuantityRemaining { get; set; }
        public string? DosageInstruction { get; set; }
        public int? ReturnedQuantity { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public List<MedicationScheduleResponseDTO> DailySchedule { get; set; } = new();
        public List<DTOs.MedicationUsageRecord.Respond.MedicationUsageRecordResponseDTO> UsageRecords { get; set; } = new();
    }
    
    public class MedicationScheduleResponseDTO
    {
        public Guid Id { get; set; }
        public TimeSpan Time { get; set; }
        public int Dosage { get; set; }
        public string? Note { get; set; }
    }
}
