namespace DTOs.MedicationLotDTOs.Response
{
    public class MedicationLotStatisticsResponseDTO
    {
        public int TotalLots { get; set; }
        public int ActiveLots { get; set; }
        public int ExpiredLots { get; set; }
        public int ExpiringInNext30Days { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
