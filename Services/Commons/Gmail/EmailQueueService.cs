using Microsoft.Extensions.Logging;

namespace Services.Commons.Gmail
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly EmailQueue _emailQueue;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(EmailQueue emailQueue, ILogger<EmailQueueService> logger)
        {
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public Task QueueEmailAsync(string email, string subject, string message)
        {
            return QueueEmailAsync(new List<string> { email }, subject, message);
        }

        public Task QueueEmailAsync(List<string> emails, string subject, string message)
        {
            try
            {
                var request = new EmailRequest
                {
                    To = emails,
                    Subject = subject,
                    Body = message
                };

                _emailQueue.EnqueueEmail(request);
                _logger.LogInformation($"Email to {string.Join(", ", emails)} queued successfully");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to queue email to {string.Join(", ", emails)}");
                throw;
            }
        }
    }
}