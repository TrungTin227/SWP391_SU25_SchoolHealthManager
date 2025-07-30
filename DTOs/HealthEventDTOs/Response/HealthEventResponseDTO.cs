namespace DTOs.HealthEventDTOs.Response
{
    public class HealthEventResponseDTO
    {
        public Guid Id { get; set; }
        public string EventCode { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public Guid? VaccinationRecordId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string EventStatus { get; set; } = string.Empty;

        public string? Location { get; set; }
        public string? InjuredBodyPartsRaw { get; set; }
        public string? Severity { get; set; }
        public string? Symptoms { get; set; }
        public DateTime? FirstAidAt { get; set; }
        public string? FirstResponderName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ParentNotifiedAt { get; set; }
        public bool? IsReferredToHospital { get; set; }
        public string? ReferralHospital { get; set; }

        public Guid ReportedBy { get; set; }
        public string ReportedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Thống kê
        public int TotalMedications { get; set; }
        public int TotalSupplies { get; set; }
    }
}