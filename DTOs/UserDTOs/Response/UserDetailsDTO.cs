namespace DTOs.UserDTOs.Response
{
    public class UserDetailsDTO
    {
        public Guid Id { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Gender { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

    }
}