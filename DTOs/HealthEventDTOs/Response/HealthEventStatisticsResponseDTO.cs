namespace DTOs.HealthEventDTOs.Response
{
    public class HealthEventStatisticsResponseDTO
    {
        public int TotalEvents { get; set; }
        public int PendingEvents { get; set; }
        public int InProgressEvents { get; set; }
        public int ResolvedEvents { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Calculated properties
        public double PendingPercentage => TotalEvents > 0 ? Math.Round((double)PendingEvents / TotalEvents * 100, 2) : 0;
        public double InProgressPercentage => TotalEvents > 0 ? Math.Round((double)InProgressEvents / TotalEvents * 100, 2) : 0;
        public double ResolvedPercentage => TotalEvents > 0 ? Math.Round((double)ResolvedEvents / TotalEvents * 100, 2) : 0;
    }
}