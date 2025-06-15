namespace Services.Commons.Gmail
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        public string SchoolPhone { get; set; }
        public string HealthDepartmentEmail { get; set; }
        public string SchoolLogoUrl { get; set; }
    }
}
