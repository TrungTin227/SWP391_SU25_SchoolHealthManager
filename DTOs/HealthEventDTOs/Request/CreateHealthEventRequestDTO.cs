using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class CreateHealthEventRequestDTO
    {
        [Required(ErrorMessage = "ID học sinh là bắt buộc")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Phân loại sự kiện là bắt buộc")]
        public EventCategory EventCategory { get; set; }

        public Guid? VaccinationRecordId { get; set; }

        [Required(ErrorMessage = "Loại sự kiện là bắt buộc")]
        public EventType EventType { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời điểm xảy ra là bắt buộc")]
        public DateTime OccurredAt { get; set; }
    }
}