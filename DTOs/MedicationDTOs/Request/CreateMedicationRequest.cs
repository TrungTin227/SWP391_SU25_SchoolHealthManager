using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicationDTOs.Request
{
    public class CreateMedicationRequest
    {
        [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên thuốc không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Đơn vị không được vượt quá 50 ký tự")]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dạng bào chế là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Dạng bào chế không được vượt quá 100 ký tự")]
        public string DosageForm { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public MedicationCategory Category { get; set; } = MedicationCategory.Emergency;

        public MedicationStatus Status { get; set; } = MedicationStatus.Active;
    }
}
