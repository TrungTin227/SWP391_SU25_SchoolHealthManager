namespace DTOs.MedicalSupplyDTOs.Request
{
    public class UpdateStockRequest
    {
        public int? CurrentStock { get; set; }
        public int? MinimumStock { get; set; }
    }
}
