namespace DTOs.UserDTOs.Request
{
    public class EmailRequest
    {
        public List<string> To { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int RetryCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? JobType { get; set; } // Để phân biệt loại email
    }
}