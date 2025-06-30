using BusinessObjects;
using BusinessObjects.Common;
using DTOs.CounselingAppointmentDTOs.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.CheckUpRecordDTOs.Requests
{
    public class CreateCheckupRecordRequestDTO
    {
        [Required(ErrorMessage = "studentId không được để trống.")]
        public Guid ScheduleId { get; set; }

        [Required(ErrorMessage = "Chiều cao là bắt buộc.")]
        public decimal HeightCm { get; set; }

        [Required(ErrorMessage = "Cân nặng là bắt buộc.")]
        public decimal WeightKg { get; set; }
        public VisionLevel VisionLeft { get; set; }
        public VisionLevel VisionRight { get; set; }
        public HearingLevel Hearing { get; set; }
        public decimal? BloodPressureDiastolic { get; set; }
        public Guid? ExaminedByNurseId { get; set; }
        public DateTime ExaminedAt { get; set; }
        public string? Remarks { get; set; }
        public CheckupRecordStatus Status { get; set; }

        // Nếu có thông tin khám lại thì truyền DTO con vào đây
        public List<CreateAppointmentForCheckup>? CounselingAppointment { get; set; }
    }

}
