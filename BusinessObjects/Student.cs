using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Student : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(20)]
        public string StudentCode { get; set; }      // Mã định danh học sinh (ví dụ: HS2025001)

        [Required, MaxLength(50)]
        public string FirstName { get; set; }        // Tên

        [Required, MaxLength(50)]
        public string LastName { get; set; }         // Họ và tên đệm

        [Required]
        public DateTime DateOfBirth { get; set; }    // Ngày sinh

        [MaxLength(20)]
        public string? Grade { get; set; }            // Khối lớp (ví dụ: “5”)

        [MaxLength(10)]
        public string? Section { get; set; }          // Lớp (ví dụ: “5A”)

        [MaxLength(150)]
        public string? Image { get; set; }            // Ảnh đại diện (URL hoặc path)

        public ICollection<Parent> Parents { get; set; }        // Quan hệ tới Parent

        public ICollection<HealthProfile> HealthProfiles { get; set; }
        public ICollection<HealthEvent> HealthEvents { get; set; }
        public ICollection<VaccinationRecord> VaccinationRecords { get; set; }
    }
}
