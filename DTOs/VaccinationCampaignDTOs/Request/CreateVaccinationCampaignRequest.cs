using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccinationCampaignDTOs.Request
{
    public class CreateVaccinationCampaignRequest
    {
        [Required(ErrorMessage = "Tên chiến dịch tiêm chủng là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên chiến dịch không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(9)]
        public string SchoolYear { get; set; } = "";

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime EndDate { get; set; }
    }
}