using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Services.Commons.Gmail
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly EmailQueue _emailQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<EmailBackgroundService> _logger;
        private const int MaxRetryCount = 3;

        public EmailBackgroundService(
            EmailQueue emailQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<EmailBackgroundService> logger)
        {
            _emailQueue = emailQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service is starting.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Lấy email từ queue (có thể ném OperationCanceledException nếu token bị huỷ)
                    var emailRequest = await _emailQueue.DequeueEmailAsync(stoppingToken);
                    if (emailRequest == null)
                        continue;

                    using var scope = _serviceScopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    try
                    {
                        await emailService.SendEmailAsync(
                            emailRequest.To,
                            emailRequest.Subject,
                            emailRequest.Body);

                        _logger.LogInformation($"Email sent to {string.Join(", ", emailRequest.To)}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending email to {string.Join(", ", emailRequest.To)}");

                        if (emailRequest.RetryCount < MaxRetryCount)
                        {
                            emailRequest.RetryCount++;
                            _emailQueue.EnqueueEmail(emailRequest);
                            _logger.LogWarning($"Requeued email for retry ({emailRequest.RetryCount}/{MaxRetryCount})");

                            // Delay có hỗ trợ hủy
                            await Task.Delay(emailRequest.RetryCount * 1000, stoppingToken);
                        }
                        else
                        {
                            _logger.LogError($"Email to {string.Join(", ", emailRequest.To)} failed after {MaxRetryCount} attempts");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Đây là huỷ service bình thường, không cần log error
                _logger.LogInformation("Email Background Service is stopping due to cancellation.");
            }
            catch (Exception ex)
            {
                // Bắt các lỗi bất ngờ khác
                _logger.LogError(ex, "Unexpected error in Email Background Service.");
            }
        }
    }
}
