namespace DTOs.HealthEventDTOs.Response
{
    public class HealthEventDetailResponseDTO : HealthEventResponseDTO
    {
        public List<EventMedicationResponseDTO> Medications { get; set; } = new();
        public List<SupplyUsageResponseDTO> Supplies { get; set; } = new();
    }
}
