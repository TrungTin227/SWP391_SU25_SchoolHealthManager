using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class TreatHealthEventRequest
    {
        //[Required] public Guid HealthEventId { get; init; }

        /* ---- thông tin sơ cứu ---- */
        [Required] public DateTime FirstAidAt { get; init; }
        [Required] public Guid FirstResponderId { get; init; }
        [MaxLength(500)] public string? FirstAidDescription { get; init; }

        /* ---- thuốc & vật tư ---- */
        public List<CreateEventMedicationRequest>? Medications { get; init; }
        public List<CreateSupplyUsageRequest>? Supplies { get; init; }

        /* ---- các trường khác liên quan đến điều trị ---- */
        [MaxLength(100)] public string? Location { get; init; }
        [MaxLength(200)] public string? InjuredBodyPartsRaw { get; init; }
        public SeverityLevel? Severity { get; init; }
        [MaxLength(500)] public string? Symptoms { get; init; }

        /* ---- chuyển viện ---- */
        public bool? IsReferredToHospital { get; init; }
        [MaxLength(200)] public string? ReferralHospital { get; init; }
        public DateTime? ReferralDepartureTime { get; init; }
        [MaxLength(50)] public string? ReferralTransportBy { get; init; }
    }
}