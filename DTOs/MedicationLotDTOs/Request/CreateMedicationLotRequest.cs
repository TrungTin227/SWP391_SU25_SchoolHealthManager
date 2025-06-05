using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicationLotDTOs.Request
{
    public class CreateMedicationLotRequest
    {
        [Required(ErrorMessage = "ID thuốc là bắt buộc")]
        public Guid MedicationId { get; set; }

        [Required(ErrorMessage = "Số lô là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Số lô không được vượt quá 100 ký tự")]
        public string LotNumber { get; set; } = "";

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        public DateTime ExpiryDate { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vị trí lưu trữ là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Vị trí lưu trữ không được vượt quá 100 ký tự")]
        public string StorageLocation { get; set; } = "";
    }
}