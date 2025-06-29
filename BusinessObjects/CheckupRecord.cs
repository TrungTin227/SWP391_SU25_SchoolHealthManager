using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class CheckupRecord : BaseEntity
    {
        [Key] public Guid Id { get; set; }
        //public Guid StudentId { get; set; }
        //[ForeignKey(nameof(StudentId))]
        //public virtual Student Student { get; set; }
        public Guid ScheduleId { get; set; }
        public CheckupSchedule Schedule { get; set; } = null!;
        public decimal HeightCm { get; set; }
        public decimal WeightKg { get; set; }
        public VisionLevel VisionLeft { get; set; } 
        public VisionLevel VisionRight { get; set; } 
        public HearingLevel Hearing { get; set; } 
        public decimal? BloodPressureDiastolic { get; set; } //huyết áp 
        public Guid? ExaminedByNurseId { get; set; }              // Y tá thực hiện
        public User? ExaminedByNurse { get; set; }
        public DateTime ExaminedAt { get; set; }                  // Thời gian khám thực tế
        public string? Remarks { get; set; }              // Khuyến nghị
        public CheckupRecordStatus Status { get; set; }           // Hoàn thành/Cần tái khám
        public virtual ICollection<CounselingAppointment>? CounselingAppointments { get; set; }

    }
}
