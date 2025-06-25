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

        [Required(ErrorMessage = "Danh sách học sinh là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một học sinh trong lịch tiêm")]
        public List<Guid> StudentIds { get; set; } = new List<Guid>();

        public string? Notes { get; set; }
    }
}