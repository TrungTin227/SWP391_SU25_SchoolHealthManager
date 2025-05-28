using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Notification : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public VaccinationSchedule Schedule { get; set; }
        public Guid ParentId { get; set; }
        public Parent Parent { get; set; }
        public DateTime? SentAt { get; set; }
        [MaxLength(20)]
        public string Status { get; set; }
        public int RetryCount { get; set; }
    }
}
