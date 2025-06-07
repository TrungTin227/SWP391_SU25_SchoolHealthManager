namespace DTOs.MedicationLotDTOs.Response
{
    public class MedicationLotDetailResponse
    {
        public Guid Id { get; set; }
        public string LotNumber { get; set; } = "";
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public string StorageLocation { get; set; } = "";
        public bool IsExpired => ExpiryDate.Date <= DateTime.UtcNow.Date;
        public int DaysUntilExpiry => (ExpiryDate.Date - DateTime.UtcNow.Date).Days;
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