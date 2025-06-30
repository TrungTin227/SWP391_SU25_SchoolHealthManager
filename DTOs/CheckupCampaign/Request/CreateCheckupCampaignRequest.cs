using System.ComponentModel.DataAnnotations;

namespace DTOs.CheckupCampaign.Request
{
    public class CreateCheckupCampaignRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        [Required, MaxLength(50)]
        public string SchoolYear { get; set; } = "";

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
