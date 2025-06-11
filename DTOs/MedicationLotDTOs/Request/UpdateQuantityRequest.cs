using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicationLotDTOs.Request
{
    public class UpdateQuantityRequest
    {
        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Quantity { get; set; }
    }
}
