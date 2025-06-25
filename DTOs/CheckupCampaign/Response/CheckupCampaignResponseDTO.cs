using BusinessObjects.Common;

namespace DTOs.CheckupCampaign.Response
{
    public class CheckupCampaignResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string SchoolYear { get; set; } = "";
        public DateTime ScheduledDate { get; set; }
        public string Description { get; set; } = "";
        public CheckupCampaignStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalSchedules { get; set; }
        public int CompletedSchedules { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = "";
    }
}
