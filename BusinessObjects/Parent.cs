using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class Parent : BaseEntity
    {
        // 1: Shared PK với User (IdentityUser<Guid>)
        [Key, ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        // Không cần StudentId nữa vì đã có quan hệ ngược lại
        // public Guid? StudentId { get; set; }
        // public Student? Student { get; set; }

        // 3: Mối quan hệ với con: Father/Mother/Guardian …
        [Required, MaxLength(50)]
        public Relationship Relationship { get; set; }

        //  Collection các students
        public ICollection<Student> Students { get; set; } = new List<Student>();

        // 4: Các hồ sơ sức khoẻ do phụ huynh tạo
        public ICollection<HealthProfile> HealthProfiles { get; set; } = new List<HealthProfile>();

        // 5: Các lần phụ huynh gửi thuốc cho trường
        public ICollection<ParentMedicationDelivery> ParentMedicationDeliveries { get; set; } = new List<ParentMedicationDelivery>();

        // 6: Các thông báo gửi đến phụ huynh (tiêm chủng, khám định kỳ…)
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // 7: (Nếu có) Những mũi tiêm phụ huynh tự ghi nhận tại nhà
        public ICollection<ParentVaccinationRecord> ParentVaccinationRecords { get; set; } = new List<ParentVaccinationRecord>();

        public virtual ICollection<CounselingAppointment> CounselingAppointments { get; set; } = new List<CounselingAppointment>();

    }
}