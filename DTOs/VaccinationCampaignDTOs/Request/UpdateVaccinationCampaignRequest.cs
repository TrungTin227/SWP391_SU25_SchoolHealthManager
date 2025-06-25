using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccinationCampaignDTOs.Request
{
    public class UpdateVaccinationCampaignRequest
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(200, ErrorMessage = "Tên chiến dịch không được vượt quá 200 ký tự")]
        public string? Name { get; set; }

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}