using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class CreateSupplyUsageRequest
    {
        //[Required(ErrorMessage = "ID lô vật tư y tế là bắt buộc")]
        public Guid MedicalSupplyLotId { get; set; }

        //[Required(ErrorMessage = "Số lượng là bắt buộc")]
        //[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int QuantityUsed { get; set; }

        //[MaxLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Notes { get; set; }
    }
}