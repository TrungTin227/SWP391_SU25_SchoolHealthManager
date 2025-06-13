using BusinessObjects.Common;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class User : IdentityUser<Guid>
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{LastName} {FirstName}".Trim();

        public Gender Gender { get; set; }
        public bool IsFirstLogin { get; set; } = true; // Mặc định là true, sẽ được set thành false khi người dùng đăng nhập lần đầu tiên

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
        public NurseProfile? StaffProfile { get; set; }
    }
}
