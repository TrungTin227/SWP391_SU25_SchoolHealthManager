using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class UpdateHealthEventRequest
    {
        [Required(ErrorMessage = "ID sự kiện y tế là bắt buộc")]
        public Guid HealthEventId { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }
        public EventStatus? EventStatus { get; set; }

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
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ParentArrivalAt { get; set; }
        [MaxLength(100)] public string? ParentReceivedBy { get; set; }

        [MaxLength(500)] public string? AdditionalNotes { get; set; }
        public string? AttachmentUrlsRaw { get; set; }
        [MaxLength(500)] public string? WitnessesRaw { get; set; }
        /// <summary>
        /// Danh sách thuốc cần thêm vào sự kiện y tế
        /// </summary>
        //public List<CreateEventMedicationRequest>? EventMedications { get; set; }

        /// <summary>
        /// Danh sách vật tư y tế cần thêm vào sự kiện
        /// </summary>
        public List<CreateSupplyUsageRequest>? SupplyUsages { get; set; }
    }
}