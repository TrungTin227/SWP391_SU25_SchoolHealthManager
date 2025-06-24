using DTOs.CheckupSchedule.Response;

namespace DTOs.CheckupCampaign.Response
{
    public class CheckupCampaignDetailResponseDTO : CheckupCampaignResponseDTO
    {
        public List<CheckupScheduleResponseDTO> Schedules { get; set; } = new();
    }
}
