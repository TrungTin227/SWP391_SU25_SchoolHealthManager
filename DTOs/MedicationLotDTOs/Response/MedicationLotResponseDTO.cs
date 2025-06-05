namespace DTOs.MedicationLotDTOs.Response
{
    public class MedicationLotResponseDTO
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; } = "";
        public string MedicationUnit { get; set; } = "";
        public string LotNumber { get; set; } = "";
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public string StorageLocation { get; set; } = "";
        public bool IsExpired => ExpiryDate.Date <= DateTime.UtcNow.Date;
        public int DaysUntilExpiry => (ExpiryDate.Date - DateTime.UtcNow.Date).Days;
        public bool IsDeleted { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = "";
        public string UpdatedBy { get; set; } = "";
    }
}