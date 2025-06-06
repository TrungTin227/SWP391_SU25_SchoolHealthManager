using BusinessObjects;
using BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserDTOs.Request
{
    public class UserRegisterRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public Gender Gender { get; set; }

        //// Audit fields
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //// Lưu Guid của User đã tạo
        //public Guid CreatedBy { get; set; }

        //public DateTime UpdatedAt { get; set; }

        //// Lưu Guid của User đã cập nhật
        //public Guid UpdatedBy { get; set; }

    }
}
