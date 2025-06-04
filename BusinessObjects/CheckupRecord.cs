using BusinessObjects.Common;
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
        public VisionLevel VisionLeft { get; set; } 
        public VisionLevel VisionRight { get; set; } 
        public HearingLevel Hearing { get; set; } 
        public decimal? BloodPressureDiastolic { get; set; } //huyết áp 
        public string? Notes { get; set; }           // phát hiện bất thường
        public bool FollowUpNeeded { get; set; }    // cần hẹn tư vấn
        public virtual ICollection<CounselingAppointment> CounselingAppointments { get; set; } = new List<CounselingAppointment>();

    }
}
