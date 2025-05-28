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
        public string Status { get; set; }  // Chưa xác nhận/Đã xác nhận
    }
}
