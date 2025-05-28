namespace DTOs.UserDTOs.Request
{
    public class EmailRequest
    {
        public List<string> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int RetryCount { get; set; } = 0;
    }
}
