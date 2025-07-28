namespace Services.Helpers
{
    public static class VaccinationValidationHelper
    {
        public static void EnsureCampaignNotCompleted(VaccinationCampaignStatus status)
        {
            if (status == VaccinationCampaignStatus.Resolved)
                throw new InvalidOperationException("Chiến dịch đã hoàn thành, không thể thực hiện thao tác.");
        }

        public static void EnsureCampaignNotCancelled(VaccinationCampaignStatus status)
        {
            if (status == VaccinationCampaignStatus.Cancelled)
                throw new InvalidOperationException("Chiến dịch đã bị hủy.");
        }

        public static void EnsureDateRange(DateTime start, DateTime end)
        {
            if (end <= start.AddDays(1))
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu ít nhất 1 ngày.");
        }

        public static void EnsureNotPast(DateTime date, string field = "Ngày")
        {
            var now = DateTime.UtcNow.Date;
            if (date < now)
                throw new ArgumentException($"{field} không được trong quá khứ.");
        }

        public static void EnsureWithinCampaign(DateTime scheduled, DateTime campaignStart, DateTime campaignEnd)
        {
            if (scheduled < campaignStart || scheduled > campaignEnd)
                throw new ArgumentOutOfRangeException("Lịch tiêm phải nằm trong thời gian chiến dịch.");
        }
    }
}