namespace Services.Commons.Gmail
{
    public interface IEmailService
    {
        Task SendEmailAsync(List<string> to, string subject, string message);
    }
}