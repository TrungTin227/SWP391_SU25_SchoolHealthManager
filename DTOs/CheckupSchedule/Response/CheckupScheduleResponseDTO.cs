using BusinessObjects.Common;

namespace DTOs.CheckupSchedule.Response
{
    public class CheckupScheduleResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = "";
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentCode { get; set; } = "";
        public string Grade { get; set; } = "";
        public string Section { get; set; } = "";
        public DateTime NotifiedAt { get; set; }
        public DateTime ScheduledAt { get; set; }
        public CheckupScheduleStatus ParentConsentStatus { get; set; }
        public DateTime? ConsentReceivedAt { get; set; }
        public string? SpecialNotes { get; set; }
        public bool HasRecord { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CheckupScheduleDetailResponseDTO : CheckupScheduleResponseDTO
    {
        public StudentBasicInfoDTO Student { get; set; } = new();
        public CheckupCampaignBasicInfoDTO Campaign { get; set; } = new();
        public CheckupRecordBasicInfoDTO? Record { get; set; }
    }

    public class StudentBasicInfoDTO
    {
        public Guid Id { get; set; }
        public string StudentCode { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Grade { get; set; } = "";
        public string Section { get; set; } = "";
        public string? Image { get; set; }
    }

    public class CheckupCampaignBasicInfoDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string SchoolYear { get; set; } = "";
        public DateTime ScheduledDate { get; set; }
        public CheckupCampaignStatus Status { get; set; }
    }

    public class CheckupRecordBasicInfoDTO
    {
        public Guid Id { get; set; }
        public CheckupRecordStatus Status { get; set; }
        public DateTime CheckupDate { get; set; }
        public string? Diagnosis { get; set; }
        public string? Recommendations { get; set; }
    }
}