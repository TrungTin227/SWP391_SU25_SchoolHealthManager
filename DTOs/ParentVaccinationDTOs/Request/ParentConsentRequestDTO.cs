using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.ParentVaccinationDTOs.Request
{
    public class ParentConsentRequestDTO
    {
        [Required]
        public Guid SessionStudentId { get; set; }

        [Required]
        public ParentConsentStatus ConsentStatus { get; set; }

        [MaxLength(1000)]
        public string? ParentNotes { get; set; }

        [Required, MaxLength(500)]
        public string ParentSignature { get; set; }
    }

    public class BatchParentConsentRequestDTO
    {
        [Required]
        public List<ParentConsentRequestDTO> Consents { get; set; } = new List<ParentConsentRequestDTO>();
    }
}