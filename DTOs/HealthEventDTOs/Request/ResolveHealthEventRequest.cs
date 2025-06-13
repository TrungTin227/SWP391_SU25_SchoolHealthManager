using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class ResolveHealthEventRequest
    {
        [Required(ErrorMessage = "ID sự kiện y tế là bắt buộc")]
        public Guid HealthEventId { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú hoàn thành không được vượt quá 500 ký tự")]
        public string? CompletionNotes { get; set; }
    }
}