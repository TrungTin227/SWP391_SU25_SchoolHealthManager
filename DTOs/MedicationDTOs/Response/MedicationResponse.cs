using BusinessObjects.Common;

namespace DTOs.MedicationDTOs.Response
{
    public class MedicationResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string DosageForm { get; set; } = string.Empty;
        public MedicationCategory Category { get; set; }
        public MedicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalLots { get; set; }
        public int TotalQuantity { get; set; }
    }
}
