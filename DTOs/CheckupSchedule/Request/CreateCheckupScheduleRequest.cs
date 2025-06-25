using BusinessObjects.Common;
using DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.CheckupSchedule.Request
{
    public class CreateCheckupScheduleRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        // Option 1: Individual students
        public List<Guid> StudentIds { get; set; } = new();

        // Option 2: By Grade/Section
        public List<string> Grades { get; set; } = new();
        public List<string> Sections { get; set; } = new();

        // Option 3: All students in specific grades
        public bool IncludeAllStudentsInGrades { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateCheckupScheduleRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        public CheckupScheduleStatus ParentConsentStatus { get; set; }

        [MaxLength(500)]
        public string? SpecialNotes { get; set; }
    }

    public class CheckupBatchUpdateStatusRequest       
    {
        [Required]
        public List<Guid> ScheduleIds { get; set; } = new();

        [Required]
        public CheckupScheduleStatus Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class CheckupBatchDeleteScheduleRequest : BatchIdsRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class CheckupBatchRestoreScheduleRequest : BatchIdsRequest
    {
    }

    public class UpdateConsentStatusRequest
    {
        [Required]
        public Guid ScheduleId { get; set; }

        [Required]
        public CheckupScheduleStatus ConsentStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}