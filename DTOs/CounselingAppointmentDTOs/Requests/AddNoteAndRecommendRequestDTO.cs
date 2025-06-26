using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.CounselingAppointmentDTOs.Requests
{
    public class AddNoteAndRecommendRequestDTO
    {
        [Required(ErrorMessage = "Id không được để trống")]
        public Guid CounselingAppointmentId { get; set; }
        [MaxLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Notes { get; set; }

        [MaxLength(1000, ErrorMessage = "Khuyến nghị không được vượt quá 1000 ký tự")]
        public string? Recommendations { get; set; }
    }
}
