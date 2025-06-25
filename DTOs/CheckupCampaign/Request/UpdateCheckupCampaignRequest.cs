using System.ComponentModel.DataAnnotations;

namespace DTOs.CheckupCampaign.Request
{
    public class UpdateCheckupCampaignRequest
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? SchoolYear { get; set; }

        public DateTime? ScheduledDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
