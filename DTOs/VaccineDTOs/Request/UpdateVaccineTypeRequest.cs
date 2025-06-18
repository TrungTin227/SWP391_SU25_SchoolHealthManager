using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccineDTOs.Request
{
    public class UpdateVaccineTypeRequest
    {
        [Required(ErrorMessage = "ID vaccine là bắt buộc")]
        public Guid Id { get; set; }

        [MaxLength(30, ErrorMessage = "Mã vaccine không được vượt quá 30 ký tự")]
        public string? Code { get; set; }

        [MaxLength(200, ErrorMessage = "Tên vaccine không được vượt quá 200 ký tự")]
        public string? Name { get; set; }

        [MaxLength(100, ErrorMessage = "Nhóm vaccine không được vượt quá 100 ký tự")]
        public string? Group { get; set; }

        [Range(0, 240, ErrorMessage = "Tuổi khuyến nghị phải từ 0 đến 240 tháng")]
        public int? RecommendedAgeMonths { get; set; }

        [Range(0, 365, ErrorMessage = "Khoảng cách tối thiểu phải từ 0 đến 365 ngày")]
        public int? MinIntervalDays { get; set; }

        public bool? IsActive { get; set; }
    }
}