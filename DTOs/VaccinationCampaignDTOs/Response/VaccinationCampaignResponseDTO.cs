using BusinessObjects.Common;

namespace DTOs.VaccinationCampaignDTOs.Response
{
    public class VaccinationCampaignResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SchoolYear { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public VaccinationCampaignStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public int TotalSchedules { get; set; }
        public int CompletedSchedules { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class VaccinationCampaignDetailResponseDTO : VaccinationCampaignResponseDTO
    {
        public List<VaccinationScheduleResponseDTO> Schedules { get; set; } = new List<VaccinationScheduleResponseDTO>();
    }

    public class VaccinationScheduleResponseDTO
    {
        public Guid Id { get; set; }
        public string VaccinationTypeName { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public ScheduleStatus ScheduleStatus { get; set; }
        public string ScheduleStatusName => ScheduleStatus.ToString();
        public int TotalStudents { get; set; }
        public int CompletedRecords { get; set; }
    }
}