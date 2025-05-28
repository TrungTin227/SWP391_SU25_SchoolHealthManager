namespace Services.Commons.Gmail
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailAsync(List<string> to, string subject, string message);
    }
}