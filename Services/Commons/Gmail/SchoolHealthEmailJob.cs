using Microsoft.Extensions.Logging;
using Quartz;

namespace Services.Commons.Gmail
{
    public class SchoolHealthEmailJob : IJob
    {
        private readonly ISchoolHealthEmailService _schoolHealthEmailService;
        private readonly ILogger<SchoolHealthEmailJob> _logger;

        public SchoolHealthEmailJob(
            ISchoolHealthEmailService schoolHealthEmailService,
            ILogger<SchoolHealthEmailJob> logger)
        {
            _schoolHealthEmailService = schoolHealthEmailService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("School Health email job started at {Time}", DateTime.Now);

            try
            {
                // Gửi báo cáo sức khỏe hàng ngày cho y tá
                var nurseEmails = new List<string> { "nurse1@school.edu.vn", "nurse2@school.edu.vn","tinvtse@gmail.com" };
                var dailyReport = GenerateDailyHealthSummary();

                await _schoolHealthEmailService.SendDailyHealthReportAsync(nurseEmails, dailyReport);

                _logger.LogInformation("Daily health report sent successfully at {Time}", DateTime.Now);

                // Kiểm tra và gửi nhắc nhở về chiến dịch sắp tới
                await CheckUpcomingCampaigns();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in school health email job at {Time}", DateTime.Now);
            }
        }

        private string GenerateDailyHealthSummary()
        {
            // Logic để tạo báo cáo sức khỏe hàng ngày
            return $@"
                <h3>Tóm tắt hoạt động y tế ngày {DateTime.Now:dd/MM/yyyy}</h3>
                <ul>
                    <li>Số lượt khám: [Sẽ lấy từ database]</li>
                    <li>Số ca cấp cứu: [Sẽ lấy từ database]</li>
                    <li>Thuốc đã sử dụng: [Sẽ lấy từ database]</li>
                    <li>Vật tư cần bổ sung: [Sẽ lấy từ database]</li>
                </ul>
            ";
        }

        private async Task CheckUpcomingCampaigns()
        {
            // Logic kiểm tra chiến dịch sắp tới và gửi nhắc nhở
            _logger.LogInformation("Checking upcoming health campaigns...");
        }
    }
}