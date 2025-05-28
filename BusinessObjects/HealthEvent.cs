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
        public string EventCategory { get; set; } = "";

        // Nếu là sự cố tiêm chủng, liên kết đến bản ghi tiêm
        public Guid? VaccinationRecordId { get; set; }
        public VaccinationRecord? VaccinationRecord { get; set; }

        // Mô tả chi tiết sự kiện (tai nạn, sốt, phản ứng dị ứng…)
        public string EventType { get; set; }

        public string Description { get; set; }

        // Thời điểm xảy ra
        public DateTime OccurredAt { get; set; }

        // Trạng thái xử lý: Pending, InProgress, Resolved…
        [MaxLength(50)]
        public string EventStatus { get; set; }

        // Ai ghi nhận
        public Guid ReportedBy { get; set; }
        public User ReportedUser { get; set; }

        // Các thuốc/vật tư đã dùng khi xử lý sự kiện
        public ICollection<EventMedication> EventMedications { get; set; }
            = new List<EventMedication>();
        public ICollection<Report> Reports { get; set; }
            = new List<Report>();
    }
}
