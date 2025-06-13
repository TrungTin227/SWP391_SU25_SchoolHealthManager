namespace DTOs.HealthEventDTOs.Response
{
    public class EventMedicationResponseDTO
    {
        public Guid Id { get; set; }
        public Guid MedicationLotId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
