using BusinessObjects;
using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.CounselingAppointmentDTOs.Requests
{
    public class CreateCounselingAppointmentRequestDTO
    {
        [Required(ErrorMessage = "Mã học sinh là bắt buộc")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Mã phụ huynh là bắt buộc")]
        public Guid ParentId { get; set; }

        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        public Guid StaffUserId { get; set; }

        [Required(ErrorMessage = "Ngày hẹn tư vấn là bắt buộc")]
        public DateTime AppointmentDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0 phút")]
        public int Duration { get; set; } = 30;


        [Required(ErrorMessage = "Mục đích là bắt buộc")]
        public string Purpose { get; set; }

        //[MaxLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        //public string? Notes { get; set; }

        //[MaxLength(1000, ErrorMessage = "Khuyến nghị không được vượt quá 1000 ký tự")]
        //public string? Recommendations { get; set; }

        public Guid? CheckupRecordId { get; set; }

        public Guid? VaccinationRecordId { get; set; }
       
    }
}
