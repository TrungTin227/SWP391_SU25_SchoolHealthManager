using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class HealthEvent : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        // Phân loại sự kiện: "General" | "Vaccination"
        [MaxLength(50)]
        public EventCategory EventCategory { get; set; }

        // Nếu là sự cố tiêm chủng, liên kết đến bản ghi tiêm
        public Guid? VaccinationRecordId { get; set; }
        public VaccinationRecord? VaccinationRecord { get; set; }

        // Mô tả chi tiết sự kiện (tai nạn, sốt, phản ứng dị ứng…)
        public EventType EventType { get; set; }

        public string Description { get; set; }

        // Thời điểm xảy ra
        public DateTime OccurredAt { get; set; }

        // Trạng thái xử lý: Pending, InProgress, Resolved…
        [MaxLength(50)]
        public EventStatus EventStatus { get; set; }

        // Ai ghi nhận
        public Guid ReportedUserId { get; set; }

        //navigation property
        public User ReportedUser { get; set; }

        // Các thuốc/vật tư đã dùng khi xử lý sự kiện
        public ICollection<EventMedication> EventMedications { get; set; }
            = new List<EventMedication>();

        // THÊM: Quan hệ với SupplyUsage
        public virtual ICollection<SupplyUsage> SupplyUsages { get; set; }
            = new List<SupplyUsage>();

        public ICollection<Report> Reports { get; set; }
            = new List<Report>();
    }
}