using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class SessionStudent : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        // Liên kết tới lịch tiêm
        public Guid VaccinationScheduleId { get; set; }
        [ForeignKey(nameof(VaccinationScheduleId))]
        public virtual VaccinationSchedule VaccinationSchedule { get; set; }

        // Liên kết tới học sinh
        public Guid StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        // Trạng thái của học sinh trong buổi tiêm này
        public SessionStatus Status { get; set; } // Present, Absent, Excused, etc.

        public DateTime? CheckInTime { get; set; }
        public string? Notes { get; set; }


        // Thời gian gửi thông báo cho phụ huynh
        public DateTime? ParentNotifiedAt { get; set; }

        // Thời gian phụ huynh ký (đồng ý hoặc từ chối)
        public DateTime? ParentSignedAt { get; set; }

        // Trạng thái đồng ý của phụ huynh
        public ParentConsentStatus ConsentStatus { get; set; } = ParentConsentStatus.Pending;

        // Chữ ký số hoặc confirmation token từ phụ huynh
        [MaxLength(500)]
        public string? ParentSignature { get; set; }

        // Ghi chú từ phụ huynh khi ký (lý do từ chối, yêu cầu đặc biệt...)
        [MaxLength(1000)]
        public string? ParentNotes { get; set; }

        // Deadline để phụ huynh ký (có thể tính từ ScheduledAt - X ngày)
        public DateTime? ConsentDeadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<VaccinationRecord> VaccinationRecords { get; set; }
            = new List<VaccinationRecord>();
    }
}