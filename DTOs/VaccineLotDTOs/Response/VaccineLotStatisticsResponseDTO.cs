namespace DTOs.VaccineLotDTOs.Response
{
    public class VaccineLotStatisticsResponseDTO
    {
        public int TotalLots { get; set; }
        public int ActiveLots { get; set; }
        public int ExpiredLots { get; set; }
        public int ExpiringInNext30Days { get; set; }
        public int DeletedLots { get; set; }
        public int TotalQuantity { get; set; }
    }
}