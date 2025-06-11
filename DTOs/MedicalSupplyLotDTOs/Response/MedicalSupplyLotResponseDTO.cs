namespace DTOs.MedicalSupplyLotDTOs.Response
{
    public class MedicalSupplyLotResponseDTO
    {
        public Guid Id { get; set; }
        public Guid MedicalSupplyId { get; set; }
        public string MedicalSupplyName { get; set; } = "";
        public string MedicalSupplyUnit { get; set; } = "";
        public string LotNumber { get; set; } = "";
        public DateTime ExpirationDate { get; set; }
        public DateTime ManufactureDate { get; set; }
        public int Quantity { get; set; }
        public bool IsExpired { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = "";
        public string UpdatedBy { get; set; } = "";
    }
}