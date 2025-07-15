using BusinessObjects.Common;

namespace DTOs.CheckupSchedule.Response
{
    public class CheckupScheduleForParentResponseDTO
    {
        public Guid Id { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public CheckupScheduleStatus ParentConsentStatus { get; set; }
    }
}
