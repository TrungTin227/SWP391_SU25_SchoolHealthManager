using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicalSupplyLotDTOs.Request
{
    public class UpdateMedicalSupplyLotRequest
    {
        [Required(ErrorMessage = "Số lô là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Số lô không được vượt quá 50 ký tự")]
        public string LotNumber { get; set; } = "";

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        public DateTime ExpirationDate { get; set; }

        [Required(ErrorMessage = "Ngày sản xuất là bắt buộc")]
        public DateTime ManufactureDate { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Quantity { get; set; }
    }
}