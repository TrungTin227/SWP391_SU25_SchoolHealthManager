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

        // 2: (Tuỳ chọn) mỗi phụ huynh gắn với một học sinh chính,
        //    hoặc để null nếu chưa khai báo.
        public Guid? StudentId { get; set; }
        public Student? Student { get; set; }

        // 3: Mối quan hệ với con: Father/Mother/Guardian …
        [Required, MaxLength(50)]
        public string Relationship { get; set; } = "";

        // 4: Các hồ sơ sức khoẻ do phụ huynh tạo
        public ICollection<HealthProfile> HealthProfiles { get; set; }
            = new List<HealthProfile>();

        // 5: Các lần phụ huynh gửi thuốc cho trường
        public ICollection<ParentMedicationDelivery> ParentMedicationDeliveries { get; set; }
            = new List<ParentMedicationDelivery>();

        // 6: Các thông báo gửi đến phụ huynh (tiêm chủng, khám định kỳ…)
        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();

        // 7: (Nếu có) Những mũi tiêm phụ huynh tự ghi nhận tại nhà
        public ICollection<ParentVaccinationRecord> ParentVaccinationRecords { get; set; }
            = new List<ParentVaccinationRecord>();
    }
}
