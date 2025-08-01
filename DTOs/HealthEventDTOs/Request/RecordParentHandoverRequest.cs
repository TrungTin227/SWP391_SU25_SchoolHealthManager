using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class RecordParentHandoverRequest
    {

        [Required]
        public DateTime ParentArrivalAt { get; set; }

        [MaxLength(500)]
        public string? ParentSignatureUrl { get; set; }
    }
}