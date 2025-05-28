using System.ComponentModel.DataAnnotations;

namespace DTOs.UserDTOs.Request
{
    public class ResendEmailRequestDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
