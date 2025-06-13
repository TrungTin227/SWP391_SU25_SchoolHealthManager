using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class CreateEventMedicationRequest
    {
        //[Required(ErrorMessage = "ID lô thuốc là bắt buộc")]
        public Guid MedicationLotId { get; set; }

        //[Required(ErrorMessage = "Số lượng là bắt buộc")]
        //[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        //[MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}