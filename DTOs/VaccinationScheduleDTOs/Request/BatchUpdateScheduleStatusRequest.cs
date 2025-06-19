using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.VaccinationScheduleDTOs.Request
{
    public class BatchUpdateScheduleStatusRequest
    {
        [Required(ErrorMessage = "Danh sách ID lịch tiêm là bắt buộc")]
        public List<Guid> ScheduleIds { get; set; } = new List<Guid>();

        [Required(ErrorMessage = "Trạng thái mới là bắt buộc")]
        public ScheduleStatus NewStatus { get; set; }

        public string? Notes { get; set; }
    }
}