using System.ComponentModel.DataAnnotations;
using BusinessObjects.Common;

namespace DTOs.UserDTOs.Request
{
    public class AdminCreateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
        public string? PasswordConfirm { get; set; }
        public string? Gender { get; set; }
        public List<string>? Roles { get; set; }
    }
}
