using DTOs.ParentVaccinationDTOs.Response;

namespace DTOs.VaccinationScheduleDTOs.Response
{
    public class ParentVaccinationSummaryDTO
    {
        public int PendingConsentCount { get; set; }
        public int UpcomingVaccinationsCount { get; set; }
        public int CompletedVaccinationsCount { get; set; }
        public int RequiresFollowUpCount { get; set; }
        public List<ParentVaccinationScheduleResponseDTO> RecentActivities { get; set; } = new List<ParentVaccinationScheduleResponseDTO>();
    }
}
