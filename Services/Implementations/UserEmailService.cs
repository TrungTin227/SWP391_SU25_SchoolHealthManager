using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Services.Interfaces.Services.Commons.User;

namespace Services.Implementations
{
    public class UserEmailService : IUserEmailService
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly ILogger<UserEmailService> _logger;

        public UserEmailService(IEmailQueueService emailQueueService, ILogger<UserEmailService> logger)
        {
            _emailQueueService = emailQueueService ?? throw new ArgumentNullException(nameof(emailQueueService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendWelcomeEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống hoặc null.", nameof(email));

            var subject = "Chào mừng bạn đến với SmartCertifyAPI!";
            var message = @"
                <html>
                <body>
                    <h2>Chào mừng!</h2>
                    <p>Cảm ơn bạn đã đăng ký tài khoản.</p>
                    <p>Chúng tôi rất vui được chào đón bạn!</p>
                    <p>Hãy bắt đầu khám phá các tính năng của chúng tôi ngay bây giờ.</p>
                </body>
                </html>
            ";
            await QueueEmailAsync(email, subject, message);
        }

        public async Task SendEmailConfirmationAsync(string email, Guid userId, string token, string confirmEmailUri)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống hoặc null.", nameof(email));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token không được để trống hoặc null.", nameof(token));
            if (string.IsNullOrWhiteSpace(confirmEmailUri))
                throw new ArgumentException("ConfirmEmailUri không được để trống hoặc null.", nameof(confirmEmailUri));

            // Encode token trước khi truyền vào URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var encodedUserId = WebUtility.UrlEncode(userId.ToString());

            var confirmLink = $"{confirmEmailUri}?userId={encodedUserId}&token={encodedToken}";

            var subject = "Xác nhận Email";
            var message = $@"
        <html>
        <body>
            <h2>Xác nhận tài khoản</h2>
            <p>Vui lòng nhấp <a href=""{confirmLink}"">vào đây</a> để xác nhận tài khoản của bạn.</p>
            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
        </body>
        </html>
    ";
            await QueueEmailAsync(email, subject, message);
        }

        public async Task ResendEmailConfirmationAsync(string email, Guid userId, string token, string confirmEmailUri)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống hoặc null.", nameof(email));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token không được để trống hoặc null.", nameof(token));
            if (string.IsNullOrWhiteSpace(confirmEmailUri))
                throw new ArgumentException("ConfirmEmailUri không được để trống hoặc null.", nameof(confirmEmailUri));

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = $"{confirmEmailUri}?userId={userId}&token={encodedToken}";
            var subject = "Xác nhận Email - Gửi lại";
            var message = $"Vui lòng nhấp <a href=\"{link}\">vào đây</a> để xác nhận tài khoản.";
            await QueueEmailAsync(email, subject, message);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token, string resetPasswordUri)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống hoặc null.", nameof(email));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token không được để trống hoặc null.", nameof(token));
            if (string.IsNullOrWhiteSpace(resetPasswordUri))
                throw new ArgumentException("ResetPasswordUri không được để trống hoặc null.", nameof(resetPasswordUri));
        
            var resetLink = $"{resetPasswordUri}?email={Uri.EscapeDataString(email)}&token={token}";
            var subject = "Đặt lại mật khẩu";
            var message = $"Nhấp <a href=\"{resetLink}\">vào đây</a> để đặt lại mật khẩu.";
            await QueueEmailAsync(email, subject, message);
        }

        public async Task SendPasswordChangedNotificationAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống hoặc null.", nameof(email));

            var subject = "Mật khẩu của bạn đã được thay đổi";
            var message = @"
                <html>
                <body>
                    <h2>Thông báo thay đổi mật khẩu</h2>
                    <p>Mật khẩu của bạn đã được thay đổi thành công.</p>
                    <p>Nếu bạn không thực hiện hành động này, vui lòng liên hệ với chúng tôi ngay lập tức.</p>
                </body>
                </html>
            ";
            await QueueEmailAsync(email, subject, message);
        }

        public async Task Send2FACodeAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống hoặc null.", nameof(email));
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Mã 2FA không được để trống hoặc null.", nameof(code));

            var subject = "Mã xác thực hai yếu tố (2FA)";
            var message = $@"
                <html>
                <body>
                    <h2>Mã xác thực hai yếu tố</h2>
                    <p>Mã xác thực của bạn là: <strong>{code}</strong></p>
                    <p>Mã này có hiệu lực trong 5 phút.</p>
                    <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                </body>
                </html>
            ";
            await QueueEmailAsync(email, subject, message);
        }

        private async Task QueueEmailAsync(string email, string subject, string message)
        {
            try
            {
                await _emailQueueService.QueueEmailAsync(email, subject, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể xếp hàng email cho {Email}", email);
                throw;
            }
        }
    }
}