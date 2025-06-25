using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccinationScheduleDTOs.Request
{
    public class CreateVaccinationScheduleRequest
    {
        [Required(ErrorMessage = "ID chiến dịch tiêm chủng là bắt buộc")]
        public Guid CampaignId { get; set; }

        [Required(ErrorMessage = "ID loại vắc-xin là bắt buộc")]
        public Guid VaccinationTypeId { get; set; }

        [Required(ErrorMessage = "Thời gian lên lịch là bắt buộc")]
        public DateTime ScheduledAt { get; set; }

        // Option 1: Individual students
        public List<Guid> StudentIds { get; set; } = new();

        // Option 2: By Grade/Section
        public List<string> Grades { get; set; } = new();
        public List<string> Sections { get; set; } = new();

        // Option 3: All students in specific grades
        public bool IncludeAllStudentsInGrades { get; set; } = false;

        public string? Notes { get; set; }

    }
}