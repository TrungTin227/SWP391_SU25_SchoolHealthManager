namespace DTOs.HealthEventDTOs.Response
{
    public class SupplyUsageResponseDTO
    {
        public Guid Id { get; set; }
        public Guid HealthEventId { get; set; }
        public Guid MedicalSupplyLotId { get; set; }
        public string MedicalSupplyName { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public int QuantityUsed { get; set; }
        public Guid NurseProfileId { get; set; }
        public string NurseName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
