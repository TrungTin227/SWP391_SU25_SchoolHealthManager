namespace DTOs.UserDTOs.Response
{
    public class UserDetailsDTO
    {
        public Guid Id { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }   // thêm

        public string? Gender { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public bool IsLockedOut { get; set; }      // thêm

        public DateTimeOffset? LockoutEnd { get; set; } // thêm chi tiết khóa

        public bool EmailConfirmed { get; set; }   // thêm

        public List<string> Roles { get; set; } = new();
    }
}