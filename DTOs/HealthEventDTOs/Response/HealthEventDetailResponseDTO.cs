namespace DTOs.HealthEventDTOs.Response
{
    public class HealthEventDetailResponseDTO : HealthEventResponseDTO
    {
        // 1. Extended first-aid info
        public string? FirstAidDescription { get; set; }

        // 2. Extended parent notification
        public string? ParentNotificationMethod { get; set; }
        public string? ParentNotificationNote { get; set; }

        // 3. Extended referral
        public DateTime? ReferralDepartureTime { get; set; }
        public string? ReferralTransportBy { get; set; }

        // 4. Parent arrival
        public string? ParentSignatureUrl { get; set; }
        public DateTime? ParentArrivalAt { get; set; }
        public string? ParentReceivedBy { get; set; }

        // 5. Extra notes & evidence
        public string? AdditionalNotes { get; set; }
        public string? AttachmentUrlsRaw { get; set; }
        public string? WitnessesRaw { get; set; }

        // 6. Detailed lists
        public List<EventMedicationResponseDTO> Medications { get; set; } = new();
        public List<SupplyUsageResponseDTO> Supplies { get; set; } = new();
    }
}