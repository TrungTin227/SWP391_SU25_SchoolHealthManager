using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class CheckupRecord : BaseEntity
    {
        [Key] public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public CheckupSchedule Schedule { get; set; } = null!;
        public decimal HeightCm { get; set; }
        public decimal WeightKg { get; set; }
        public string VisionLeft { get; set; } = "";
        public string VisionRight { get; set; } = "";
        public string Hearing { get; set; } = "";
        public decimal? BloodPressureDiastolic { get; set; } //huyết áp 
        public string? Notes { get; set; }           // phát hiện bất thường
        public bool FollowUpNeeded { get; set; }    // cần hẹn tư vấn
    }
}
