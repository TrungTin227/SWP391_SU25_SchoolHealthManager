using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class CheckupCampaign : BaseEntity
    {
        [Key] public Guid Id { get; set; }
        public string SchoolYear { get; set; } = "";      // e.g. "2024-2025"
        public DateTime ScheduledDate { get; set; } // ngày dự kiến khám
        public string Description { get; set; }     // ghi chú chung

        [MaxLength(200)]
        public string Name { get; set; } = "";                    // Tên chiến dịch
        public CheckupCampaignStatus Status { get; set; }         // Trạng thái workflow
        public DateTime? StartDate { get; set; }                  // Ngày bắt đầu thực tế
        public DateTime? EndDate { get; set; }                    // Ngày kết thúc
        public ICollection<CheckupSchedule> Schedules { get; set; } = new List<CheckupSchedule>();
    }
}
