using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Notification : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        // chung cho mọi loại
        public string Type { get; set; }

        // nếu Type == VaccinationSchedule
        public Guid? ScheduleId { get; set; }
        public virtual VaccinationSchedule Schedule { get; set; }

        public Guid? ParentId { get; set; }
        public virtual Parent Parent { get; set; }

        // nếu Type == SupplyShortage
        public Guid? MedicalSupplyId { get; set; }
        public virtual MedicalSupply MedicalSupply { get; set; }

        // dùng chung
        public DateTime? SentAt { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }

        public int RetryCount { get; set; }
    }
}