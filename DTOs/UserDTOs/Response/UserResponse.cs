namespace DTOs.UserDTOs.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string PhoneNumbers { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
