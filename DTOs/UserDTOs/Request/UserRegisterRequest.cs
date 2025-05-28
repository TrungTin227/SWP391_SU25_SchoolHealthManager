using System.ComponentModel.DataAnnotations;
using BusinessObjects.Common;

namespace DTOs.UserDTOs.Request
{
    public class UserRegisterRequest
    {
        public string? FirstName { get;  set; }
        public string? LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
        public string? PasswordConfirm { get; set; }
        public Gender? Gender { get; set; }
    }
}
