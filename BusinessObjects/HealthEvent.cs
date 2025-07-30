using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [ForeignKey(nameof(VaccinationRecordId))]
        public virtual VaccinationRecord? VaccinationRecord { get; set; }

        public Guid? CheckupRecordId { get; set; }

        [ForeignKey(nameof(CheckupRecordId))]
        public virtual CheckupRecord? CheckupRecord { get; set; }

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

        [MaxLength(100)]
        public string? Location { get; set; }          // “Sân chơi tầng 1”, “Lớp 3A”…

        // 2. Vị trí thương tích (cho phép đa chọn)
        [MaxLength(200)]
        public string? InjuredBodyPartsRaw { get; set; } // "Head;LeftKnee"
        // 3. Mức độ nghiêm trọng
        public SeverityLevel? Severity { get; set; }   // Enum: Minor, Moderate, Severe

        // 4. Triệu chứng lúc phát hiện (lưu JSON hoặc bảng liên kết)
        public string? Symptoms { get; set; }          // “Chảy máu; Bầm tím; Nôn 1 lần”

        // 5. Thời gian & người xử lý đầu tiên
        public DateTime? FirstAidAt { get; set; }
        public Guid? FirstResponderId { get; set; }
        [ForeignKey(nameof(FirstResponderId))]
        public NurseProfile? FirstResponder { get; set; }

        // 6. Mô tả sơ cứu
        [MaxLength(500)]
        public string? FirstAidDescription { get; set; }

        // 7. Thông báo phụ huynh
        public DateTime? ParentNotifiedAt { get; set; }
        [MaxLength(50)]
        public string? ParentNotificationMethod { get; set; } // “Phone”, “Zalo”, “Email”…
        [MaxLength(200)]
        public string? ParentNotificationNote { get; set; }

        // 8. Quyết định chuyển viện
        public bool? IsReferredToHospital { get; set; }
        [MaxLength(200)]
        public string? ReferralHospital { get; set; }
        public DateTime? ReferralDepartureTime { get; set; }
        [MaxLength(50)]
        public string? ReferralTransportBy { get; set; } // “Ambulance”, “Parent car”…

        // 9. Chữ ký / xác nhận của phụ huynh (lưu đường dẫn file ảnh)
        [MaxLength(500)]
        public string? ParentSignatureUrl { get; set; }

        // 10. Thời tiết / nhân chứng (ghi chú tự do)
        [MaxLength(500)]
        public string? AdditionalNotes { get; set; }

        // 11. Ảnh hiện trường
        public string? AttachmentUrlsRaw { get; set; } // JSON array ["url1","url2"]
        [MaxLength(20)]
        public string? EventCode { get; set; }               // EV-yyyymmdd-seq

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ParentArrivalAt { get; set; }
        [MaxLength(100)]
        public string? ParentReceivedBy { get; set; }        // Tên y-tá/giáo-viên bàn giao

        [MaxLength(500)]
        public string? WitnessesRaw { get; set; }            // JSON họ tên lớp/em chứng 

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