using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccinationCampaignDTOs.Request
{
    public class BatchVaccinationCampaignRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "Danh sách ID không được để trống")]
        public List<Guid> CampaignIds { get; set; } = new List<Guid>();
    }

    public class UpdateCampaignStatusRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public VaccinationCampaignStatus Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class BatchUpdateCampaignStatusRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "Danh sách cập nhật không được để trống")]
        public List<UpdateCampaignStatusRequest> Updates { get; set; } = new List<UpdateCampaignStatusRequest>();
    }
}