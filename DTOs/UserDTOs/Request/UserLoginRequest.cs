using System.ComponentModel.DataAnnotations;

namespace DTOs.UserDTOs.Request
{
    public class UserLoginRequest
    {
        [Required]
        public string Login { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
    