namespace Services.Commons.Gmail
{
    public interface IEmailQueueService
    {
        Task QueueEmailAsync(string email, string subject, string message);
        Task QueueEmailAsync(List<string> emails, string subject, string message);
    }
}