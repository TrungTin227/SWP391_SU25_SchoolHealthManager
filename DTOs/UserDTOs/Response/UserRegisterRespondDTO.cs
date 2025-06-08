using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserDTOs.Response
{
    public class UserRegisterRespondDTO
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public string? Gender { get; set; }
    }
}
