using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class ResolveHealthEventRequest
    {

        [MaxLength(500, ErrorMessage = "Ghi chú hoàn thành không được vượt quá 500 ký tự")]
        public string? CompletionNotes { get; set; }
    }
}