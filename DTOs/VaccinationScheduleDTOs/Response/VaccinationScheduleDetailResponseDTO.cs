using DTOs.VaccinationCampaignDTOs.Response;

namespace DTOs.VaccinationScheduleDTOs.Response
{
    public class VaccinationScheduleDetailResponseDTO : VaccinationScheduleResponseDTO
    {
        public string CampaignName { get; set; } = string.Empty;
        public string VaccinationTypeCode { get; set; } = string.Empty;
        public List<SessionStudentResponseDTO> SessionStudents { get; set; } = new List<SessionStudentResponseDTO>();
        public List<VaccinationRecordSummaryDTO> Records { get; set; } = new List<VaccinationRecordSummaryDTO>();
        // Thêm thống kê consent
        public int PendingConsentCount { get; set; }
        public int ApprovedConsentCount { get; set; }
        public int RejectedConsentCount { get; set; }
        // vaccine dự kiến
        public int VaccineExpectedCount { get; set; }
    }
}
