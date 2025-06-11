using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicalSupplyDTOs.Request
{
    public class UpdateMedicalSupplyRequest
    {
        [Required(ErrorMessage = "Tên vật tư y tế là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên vật tư y tế không được vượt quá 200 ký tự")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Đơn vị tính không được vượt quá 50 ký tự")]
        public string Unit { get; set; } = "";

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho tối thiểu không được âm")]
        public int MinimumStock { get; set; }

        public bool IsActive { get; set; }
    }
}