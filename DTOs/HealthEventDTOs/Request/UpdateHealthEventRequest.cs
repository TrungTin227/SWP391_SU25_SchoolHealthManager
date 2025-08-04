using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class UpdateHealthEventRequest
    {
        [Required]
        public EventCategory EventCategory { get; set; }
        [Required]
        public EventType EventType { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }
        [MaxLength(200)]
        public string? InjuredBodyPartsRaw { get; set; }
        public SeverityLevel? Severity { get; set; }
        [MaxLength(500)]
        public string? Symptoms { get; set; }
        public Guid? VaccinationRecordId { get; set; } 
        [MaxLength(500)]
        public string? AdditionalNotes { get; set; }
        [MaxLength(500)]
        public string? WitnessesRaw { get; set; }
    }
}