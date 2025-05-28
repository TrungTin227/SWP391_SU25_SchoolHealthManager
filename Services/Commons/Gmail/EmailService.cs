using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Services.Commons.Gmail
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            await SendEmailAsync(new List<string> { email }, subject, message);
        }

        public async Task SendEmailAsync(List<string> to, string subject, string message)
        {
            if (to == null || to.Count == 0)
                throw new ArgumentException("Recipient list cannot be empty", nameof(to));
            if (string.IsNullOrEmpty(subject))
                throw new ArgumentException("Subject cannot be empty", nameof(subject));
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be empty", nameof(message));

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));

            foreach (var recipient in to)
            {
                email.To.Add(MailboxAddress.Parse(recipient));
            }

            email.Subject = subject;
            email.Body = new TextPart("html") { Text = message };

            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {string.Join(", ", to)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {string.Join(", ", to)}");
                throw;
            }
        }
    }
}