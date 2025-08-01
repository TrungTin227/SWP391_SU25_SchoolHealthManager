using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    /// <summary>
    /// Hồ sơ ghi nhận sự kiện y tế – bước đầu tiên.
    /// Áp dụng cho học sinh tại TPHCM theo Quyết định 28/2016/QĐ-UBND
    /// và hướng dẫn Sở GD-ĐT TPHCM.
    /// </summary>
    public class CreateHealthEventRequestDTO
    {
        [Required(ErrorMessage = "Mã học sinh là bắt buộc")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Phân loại sự kiện là bắt buộc")]
        public EventCategory EventCategory { get; set; }

        [Required(ErrorMessage = "Loại sự kiện là bắt buộc")]
        public EventType EventType { get; set; }

        [Required(ErrorMessage = "Mô tả sự việc là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời điểm xảy ra là bắt buộc")]
        public DateTime OccurredAt { get; set; }

        [MaxLength(100)] public string? Location { get; set; }

        [MaxLength(200)] public string? InjuredBodyPartsRaw { get; set; }

        public SeverityLevel? Severity { get; set; }

        [MaxLength(500)] public string? Symptoms { get; set; }

        /// <summary>
        /// ID của bản ghi tiêm chủng liên quan (chỉ điền khi EventCategory = Vaccination).
        /// </summary>
        public Guid? VaccinationRecordId { get; set; }
    }
}