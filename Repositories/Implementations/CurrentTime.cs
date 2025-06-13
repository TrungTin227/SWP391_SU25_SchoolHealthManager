namespace Repositories.Implementations
{
    public class CurrentTime : ICurrentTime
    {
        private static readonly TimeZoneInfo VietnamZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows

        public DateTime GetCurrentTime()
            => DateTime.UtcNow; // hoặc DateTime.Now tuỳ nhu cầu

        public DateTime GetVietnamTime()
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamZone);
    }
}
