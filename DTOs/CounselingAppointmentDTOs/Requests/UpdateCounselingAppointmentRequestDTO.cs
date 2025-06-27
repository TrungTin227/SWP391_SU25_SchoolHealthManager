using System;
using System.ComponentModel.DataAnnotations;

namespace DTOs.CounselingAppointmentDTOs.Requests
{
    public class UpdateCounselingAppointmentRequestDTO
    {
        [Required(ErrorMessage = "Id lịch tư vấn là bắt buộc")]
        public Guid Id { get; set; }

        public Guid? StudentId { get; set; }

        public Guid? ParentId { get; set; }

        public Guid? StaffUserId { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Ngày hẹn không hợp lệ")]
        public DateTime? AppointmentDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0")]
        public int? Duration { get; set; }

        [MaxLength(500, ErrorMessage = "Mục đích không được vượt quá 500 ký tự")]
        public string? Purpose { get; set; }

        public Guid? CheckupRecordId { get; set; }

        public Guid? VaccinationRecordId { get; set; }
    }
}
    