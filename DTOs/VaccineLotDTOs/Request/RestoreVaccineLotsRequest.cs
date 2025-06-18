using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccineLotDTOs.Request
{
    public class RestoreVaccineLotsRequest
    {
        [Required(ErrorMessage = "Danh sách ID là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 ID")]
        public List<Guid> Ids { get; set; } = new();
    }
}