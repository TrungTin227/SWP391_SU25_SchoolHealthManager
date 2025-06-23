using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.ParentVaccinationDTOs.Request
{
    public class ReportVaccinationReactionRequestDTO
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid VaccinationRecordId { get; set; }

        [Required]
        public VaccinationReactionSeverity Severity { get; set; }

        [Required, MaxLength(1000)]
        public string Description { get; set; }

        public DateTime? OnsetTime { get; set; }

        [MaxLength(500)]
        public string? Actions { get; set; }

        public bool RequiresMedicalAttention { get; set; }
    }
}