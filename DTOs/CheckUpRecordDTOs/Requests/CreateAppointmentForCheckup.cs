using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.CheckUpRecordDTOs.Requests
{
    public class CreateAppointmentForCheckup
    {
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        public Guid StaffUserId { get; set; }

        [Required(ErrorMessage = "Ngày hẹn tư vấn là bắt buộc")]
        public DateTime AppointmentDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0 phút")]
        public int Duration { get; set; } = 30;


        [Required(ErrorMessage = "Mục đích là bắt buộc")]
        public string Purpose { get; set; }
    }
}
