namespace DTOs.MedicationLotDTOs.Response
{
    public class MedicationLotStatisticsResponseDTO
    {
        public int TotalLots { get; set; }
        public int ActiveLots { get; set; }
        public int ExpiredLots { get; set; }
        public int ExpiringInNext30Days { get; set; }
        public DateTime GeneratedAt { get; set; }
        /// <summary>
        /// Tỷ lệ phần trăm lô thuốc đang hoạt động
        /// </summary>
        public double ActivePercentage => TotalLots > 0 ? (double)ActiveLots / TotalLots * 100 : 0;

        /// <summary>
        /// Tỷ lệ phần trăm lô thuốc đã hết hạn
        /// </summary>
        public double ExpiredPercentage => TotalLots > 0 ? (double)ExpiredLots / TotalLots * 100 : 0;

        /// <summary>
        /// Tỷ lệ phần trăm lô thuốc sắp hết hạn
        /// </summary>
        public double ExpiringPercentage => TotalLots > 0 ? (double)ExpiringInNext30Days / TotalLots * 100 : 0;

    }
}
