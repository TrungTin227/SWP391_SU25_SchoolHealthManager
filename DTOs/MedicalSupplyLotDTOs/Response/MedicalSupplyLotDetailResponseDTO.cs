namespace DTOs.MedicalSupplyLotDTOs.Response
{
    public class MedicalSupplyLotDetailResponseDTO
    {
        public Guid Id { get; set; }
        public string LotNumber { get; set; } = "";
        public DateTime ExpirationDate { get; set; }
        public DateTime ManufactureDate { get; set; }
        public int Quantity { get; set; }
        public bool IsExpired => ExpirationDate.Date <= DateTime.UtcNow.Date;
        public int DaysUntilExpiry => (ExpirationDate.Date - DateTime.UtcNow.Date).Days;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Thông tin trạng thái lô
        public string ExpiryStatus
        {
            get
            {
                if (IsExpired) return "Hết hạn";
                if (DaysUntilExpiry <= 30) return "Sắp hết hạn";
                return "Còn hạn";
            }
        }
    }
}