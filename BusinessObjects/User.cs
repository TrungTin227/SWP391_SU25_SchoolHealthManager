using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BusinessObjects
{
    public class User : IdentityUser<Guid>
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public string? Gender { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Lưu Guid của User đã tạo
        public Guid CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Lưu Guid của User đã cập nhật
        public Guid UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }

        // Navigation đến thông tin phụ huynh và nhân viên (nullable nếu chưa gán)
        public Parent? Parent { get; set; }
        public StaffProfile? StaffProfile { get; set; }
    }
}
