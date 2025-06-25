using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccineDoseInfoDTOs.Request
{
    public class CreateVaccineDoseInfoRequest
    {
        [Required(ErrorMessage = "ID loại vaccine là bắt buộc")]
        public Guid VaccineTypeId { get; set; }

        [Required(ErrorMessage = "Số mũi tiêm là bắt buộc")]
        [Range(1, 10, ErrorMessage = "Số mũi tiêm phải từ 1 đến 10")]
        public int DoseNumber { get; set; }

        [Range(0, 1200, ErrorMessage = "Tuổi khuyến nghị phải từ 0 đến 1200 tháng")]
        public int RecommendedAgeMonths { get; set; }

        [Range(0, 3650, ErrorMessage = "Khoảng cách tối thiểu phải từ 0 đến 3650 ngày")]
        public int MinIntervalDays { get; set; }

        public Guid? PreviousDoseId { get; set; }
    }
}