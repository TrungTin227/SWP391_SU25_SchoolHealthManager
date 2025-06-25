using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccineLotDTOs.Request
{
    public class UpdateVaccineLotRequest
    {
        [Required(ErrorMessage = "Số lô là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Số lô không được vượt quá 100 ký tự")]
        public string LotNumber { get; set; } = "";

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        public DateTime ExpiryDate { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vị trí lưu trữ là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Vị trí lưu trữ không được vượt quá 100 ký tự")]
        public string StorageLocation { get; set; } = "";
    }
}