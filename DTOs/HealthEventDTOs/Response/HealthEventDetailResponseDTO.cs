namespace DTOs.HealthEventDTOs.Response
{
    public class HealthEventDetailResponseDTO : HealthEventResponseDTO
    {
        public string? FirstAidDescription { get; set; }
        public string? ParentNotificationMethod { get; set; }
        public string? ParentNotificationNote { get; set; }
        public DateTime? ReferralDepartureTime { get; set; }
        public string? ReferralTransportBy { get; set; }
        public string? ParentSignatureUrl { get; set; }
        public DateTime? ParentArrivalAt { get; set; }
        public string? ParentReceivedBy { get; set; }
        public string? AdditionalNotes { get; set; }
        public string? AttachmentUrlsRaw { get; set; }
        public string? WitnessesRaw { get; set; }
        public List<EventMedicationResponseDTO> Medications { get; set; } = new();
        public List<SupplyUsageResponseDTO> Supplies { get; set; } = new();
    }
}
