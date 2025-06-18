using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccineDTOs.Request
{
    public class CreateVaccineTypeRequest
    {
        [Required(ErrorMessage = "Mã vaccine là bắt buộc")]
        [MaxLength(30, ErrorMessage = "Mã vaccine không được vượt quá 30 ký tự")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên vaccine là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên vaccine không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Nhóm vaccine không được vượt quá 100 ký tự")]
        public string Group { get; set; } = string.Empty;

        [Range(0, 1200, ErrorMessage = "Tuổi khuyến nghị phải từ 0 đến 1200 tháng")]
        public int RecommendedAgeMonths { get; set; }

        [Range(0, 3650, ErrorMessage = "Khoảng cách tối thiểu phải từ 0 đến 3650 ngày")]
        public int MinIntervalDays { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}