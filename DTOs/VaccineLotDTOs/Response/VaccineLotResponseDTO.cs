namespace DTOs.VaccineLotDTOs.Response
{
    public class VaccineLotResponseDTO
    {
        public Guid Id { get; set; }
        public Guid VaccineTypeId { get; set; }
        public string VaccineTypeName { get; set; } = "";
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