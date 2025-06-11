using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicalSupplyLotDTOs.Request
{
    public class CreateMedicalSupplyLotRequest
    {
        [Required(ErrorMessage = "ID vật tư y tế là bắt buộc")]
        public Guid MedicalSupplyId { get; set; }

        [Required(ErrorMessage = "Số lô là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Số lô không được vượt quá 50 ký tự")]
        public string LotNumber { get; set; } = "";

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        public DateTime ExpirationDate { get; set; }

        [Required(ErrorMessage = "Ngày sản xuất là bắt buộc")]
        public DateTime ManufactureDate { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}