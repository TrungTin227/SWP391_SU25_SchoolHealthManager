using System.ComponentModel.DataAnnotations;

namespace DTOs.CheckupCampaign.Request
{
    public class CreateCheckupCampaignRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        [Required, MaxLength(50)]
        public string SchoolYear { get; set; } = "";

        [Required]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Tạo lịch cho các học sinh
        public List<Guid> StudentIds { get; set; } = new();

        // Hoặc tạo cho cả lớp/khối
        public List<string> Grades { get; set; } = new();
        public List<string> Sections { get; set; } = new();
    }
}
