using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class CreateHealthEventRequestDTO
    {
        [Required(ErrorMessage = "ID học sinh là bắt buộc")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Phân loại sự kiện là bắt buộc")]
        public EventCategory EventCategory { get; set; }

        public Guid? VaccinationRecordId { get; set; }

        [Required(ErrorMessage = "Loại sự kiện là bắt buộc")]
        public EventType EventType { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời điểm xảy ra là bắt buộc")]
        public DateTime OccurredAt { get; set; }
        [MaxLength(100)] public string? Location { get; set; }
        [MaxLength(200)] public string? InjuredBodyPartsRaw { get; set; }
        public SeverityLevel? Severity { get; set; }
        [MaxLength(500)] public string? Symptoms { get; set; }

        public DateTime? FirstAidAt { get; set; }
        public Guid? FirstResponderId { get; set; }
        [MaxLength(500)] public string? FirstAidDescription { get; set; }

        public DateTime? ParentNotifiedAt { get; set; }
        [MaxLength(50)] public string? ParentNotificationMethod { get; set; }
        [MaxLength(200)] public string? ParentNotificationNote { get; set; }

        public bool? IsReferredToHospital { get; set; }
        [MaxLength(200)] public string? ReferralHospital { get; set; }
        public DateTime? ReferralDepartureTime { get; set; }
        [MaxLength(50)] public string? ReferralTransportBy { get; set; }

        [MaxLength(500)] public string? ParentSignatureUrl { get; set; }
        [MaxLength(500)] public string? AdditionalNotes { get; set; }
        public string? AttachmentUrlsRaw { get; set; }   // JSON
        [MaxLength(500)] public string? WitnessesRaw { get; set; }   // JSON
    }
}