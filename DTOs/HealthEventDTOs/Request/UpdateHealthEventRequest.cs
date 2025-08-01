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
        [Required]
        public DateTime OccurredAt { get; set; }

        // Các trường có thể cập nhật
        [MaxLength(100)]
        public string? Location { get; set; }
        [MaxLength(200)]
        public string? InjuredBodyPartsRaw { get; set; }
        public SeverityLevel? Severity { get; set; }
        [MaxLength(500)]
        public string? Symptoms { get; set; }
        public Guid? VaccinationRecordId { get; set; }
        public DateTime? FirstAidAt { get; set; }
        public Guid? FirstResponderId { get; set; }
        [MaxLength(500)]
        public string? FirstAidDescription { get; set; }
        public DateTime? ParentNotifiedAt { get; set; }
        [MaxLength(50)]
        public string? ParentNotificationMethod { get; set; }
        [MaxLength(200)]
        public string? ParentNotificationNote { get; set; }
        public bool? IsReferredToHospital { get; set; }
        [MaxLength(200)]
        public string? ReferralHospital { get; set; }
        [MaxLength(500)]
        public string? AdditionalNotes { get; set; }
        public string? AttachmentUrlsRaw { get; set; }
        [MaxLength(500)]
        public string? WitnessesRaw { get; set; }
    }
}