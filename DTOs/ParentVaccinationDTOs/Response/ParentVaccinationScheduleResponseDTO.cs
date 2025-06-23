using BusinessObjects.Common;

namespace DTOs.ParentVaccinationDTOs.Response
{
    public class ParentVaccinationScheduleResponseDTO
    {
        public Guid Id { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string VaccinationTypeName { get; set; } = string.Empty;
        public string VaccineDescription { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public ScheduleStatus ScheduleStatus { get; set; }
        public string ScheduleStatusName => ScheduleStatus.ToString();

        // Thông tin về học sinh
        public List<ParentStudentVaccinationDTO> Students { get; set; } = new List<ParentStudentVaccinationDTO>();

        // Trạng thái tổng quan cho phụ huynh
        public ParentActionStatus ActionStatus { get; set; }
        public string ActionStatusName => ActionStatus.ToString();

        public DateTime? ConsentDeadline { get; set; }
        public int PendingConsentCount { get; set; }
        public int ApprovedConsentCount { get; set; }
        public int RejectedConsentCount { get; set; }
    }

    public class ParentStudentVaccinationDTO
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;

        // Thông tin về session
        public Guid SessionStudentId { get; set; }
        public ParentConsentStatus ConsentStatus { get; set; }
        public string ConsentStatusName => ConsentStatus.ToString();
        public DateTime? ParentSignedAt { get; set; }
        public string? ParentNotes { get; set; }
        public DateTime? ConsentDeadline { get; set; }

        // Thông tin tiêm (nếu đã tiêm)
        public bool IsVaccinated { get; set; }
        public DateTime? VaccinatedAt { get; set; }
        public string? VaccinationNotes { get; set; }
        public VaccinationReactionSeverity? ReactionSeverity { get; set; }
        public string? ReactionNotes { get; set; }
        public bool RequiresFollowUp { get; set; }
    }

    public enum ParentActionStatus
    {
        PendingConsent = 0,    // Cần ký đồng ý
        Approved = 1,          // Đã đồng ý, chờ tiêm
        Completed = 2,         // Đã hoàn thành
        RequiresFollowUp = 3,  // Cần theo dõi
        Mixed = 4              // Có nhiều trạng thái khác nhau (nhiều con)
    }
}