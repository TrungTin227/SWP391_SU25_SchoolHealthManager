using BusinessObjects.Common;

namespace DTOs.HealthEventDTOs.Response
{
    public class HealthEventResponseDTO
    {
        // 1. Identification
        public Guid Id { get; set; }
        public string EventCode { get; set; } = string.Empty;

        // 2. Student
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;

        // 3. Event core
        public string EventCategory { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string EventStatus { get; set; } = string.Empty;

        // 4. Optional vaccination link
        public Guid? VaccinationRecordId { get; set; }

        // 5. Location & injury details
        public string? Location { get; set; }
        public string? InjuredBodyPartsRaw { get; set; }
        public string? Severity { get; set; }
        public string? Symptoms { get; set; }

        // 6. First aid
        public DateTime? FirstAidAt { get; set; }
        public string? FirstResponderName { get; set; }
        public string? FirstAidDescription { get; set; }


        // 7. Parent notification
        public DateTime? ParentNotifiedAt { get; set; }
        public ParentAcknowledgmentStatus ParentAcknowledgmentStatus { get; set; } = ParentAcknowledgmentStatus.None;
        public DateTime? ParentAcknowledgedAt { get; set; }

        // 8. Referral
        public bool? IsReferredToHospital { get; set; }
        public string? ReferralHospital { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // 9. Audit
        public Guid ReportedBy { get; set; }
        public string ReportedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // 11. Additional Information (bổ sung)
        public string? AdditionalNotes { get; set; }    
        public string? AttachmentUrlsRaw { get; set; } 
        public string? WitnessesRaw { get; set; }

        // 10. Statistics
        public int TotalMedications { get; set; }
        public int TotalSupplies { get; set; }
    }
}