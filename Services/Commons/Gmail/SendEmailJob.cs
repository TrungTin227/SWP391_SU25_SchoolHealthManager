using Microsoft.Extensions.Logging;
using Quartz;

namespace Services.Commons.Gmail
{
    public class SendEmailJob : IJob
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly ILogger<SendEmailJob> _logger;

        public SendEmailJob(IEmailQueueService emailQueueService, ILogger<SendEmailJob> logger)
        {
            _emailQueueService = emailQueueService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Scheduled email job started at {Time}", DateTime.Now);

            try
            {
                var emails = new List<string> { "approver1@example.com", "approver2@example.com" };
                var subject = "Daily Reminder: Pending Approvals";
                var message = GenerateReminderEmailBody();

                if (emails.Count > 0)
                {
                    await _emailQueueService.QueueEmailAsync(emails, subject, message);
                    _logger.LogInformation("Scheduled emails queued successfully at {Time}", DateTime.Now);
                }
                else
                {
                    _logger.LogWarning("No recipients found for scheduled email at {Time}", DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in scheduled email job at {Time}", DateTime.Now);
            }
        }

        private string GenerateReminderEmailBody()
        {
            return @"
                <html>
                <body>
                    <h2>Daily Reminder</h2>
                    <p>You have pending approval requests that require your attention.</p>
                    <p>Please log in to the system to review them.</p>
                    <p>Thank you,<br>System Administrator</p>
                </body>
                </html>
            ";
        }
    }
}