using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class CheckupSchedule : BaseEntity
    {
        [Key] public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public CheckupCampaign Campaign { get; set; } = null!;
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public DateTime NotifiedAt { get; set; }    // gửi phiếu
        public DateTime ScheduledAt { get; set; }                 // Giờ khám cụ thể
        public CheckupScheduleStatus ParentConsentStatus { get; set; }          // Đồng ý của phụ huynh
        public DateTime? ConsentReceivedAt { get; set; }          // Thời gian nhận đồng ý
        public string? SpecialNotes { get; set; }                 // Ghi chú đặc biệt

        public CheckupRecord? Record { get; set; }                // 1-1 relationship
    }
}
