using Microsoft.AspNetCore.Identity;

namespace BusinessObjects
{
    public class Role : IdentityRole<Guid>
    {
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Lưu Guid của User đã tạo
        public Guid CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Lưu Guid của User đã cập nhật
        public Guid UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}
