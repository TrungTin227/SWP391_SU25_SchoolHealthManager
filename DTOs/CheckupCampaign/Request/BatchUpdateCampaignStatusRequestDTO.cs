using BusinessObjects.Common;
using DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.CheckupCampaign.Request
{
    public class BatchUpdateCampaignStatusRequestDTO
    {
        [Required]
        public List<Guid> CampaignIds { get; set; } = new();

        [Required]
        public CheckupCampaignStatus Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class BatchDeleteCampaignRequestDTO : BatchIdsRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class BatchRestoreCampaignRequestDTO : BatchIdsRequest
    {

    }
}