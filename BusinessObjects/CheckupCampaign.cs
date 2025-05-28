using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class CheckupCampaign : BaseEntity
    {
        [Key] public Guid Id { get; set; }
        public string SchoolYear { get; set; } = "";      // e.g. "2024-2025"
        public DateTime ScheduledDate { get; set; } // ngày dự kiến khám
        public string Description { get; set; }     // ghi chú chung
        public ICollection<CheckupSchedule> Schedules { get; set; } = new List<CheckupSchedule>();
    }
}
