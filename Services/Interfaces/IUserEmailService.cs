namespace Services.Interfaces
{
    namespace Services.Commons.User
    {
        public interface IUserEmailService
        {
            Task SendWelcomeEmailAsync(string email);
            Task SendEmailConfirmationAsync(string email, Guid userId, string token, string confirmEmailUri);
            Task ResendEmailConfirmationAsync(string email, Guid userId, string token, string confirmEmailUri);
            Task SendPasswordResetEmailAsync(string email, string token, string resetPasswordUri);
            Task SendPasswordChangedNotificationAsync(string email);
            Task Send2FACodeAsync(string email, string code);
        }
    }
}
