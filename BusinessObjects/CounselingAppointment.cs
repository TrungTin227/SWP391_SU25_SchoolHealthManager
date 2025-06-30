using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class CounselingAppointment : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Học sinh được hẹn tư vấn
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        /// <summary>
        /// Phụ huynh liên hệ
        /// </summary>
        [Required]
        public Guid ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public virtual Parent Parent { get; set; }

        /// <summary>
        /// Nhân viên y tế tư vấn
        /// </summary>
        [Required]
        public Guid StaffUserId { get; set; }
        [ForeignKey(nameof(StaffUserId))]
        public virtual NurseProfile StaffUser { get; set; }

        /// <summary>
        /// Nếu phát sinh từ kết quả khám bất thường
        /// </summary>
        public Guid? CheckupRecordId { get; set; }
        [ForeignKey(nameof(CheckupRecordId))]
        public virtual CheckupRecord CheckupRecord { get; set; }

        /// <summary>
        /// Ngày giờ hẹn
        /// </summary>
        [Required]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Thời lượng dự kiến tính theo phút
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Mục đích tư vấn
        /// </summary>
        [MaxLength(500)] // Giới hạn chiều dài cho SQL
        public string? Purpose { get; set; } // C# bắt buộc gán


        /// <summary>
        /// Trạng thái: Scheduled, Completed, Cancelled
        /// </summary>
        public ScheduleStatus Status { get; set; }

        /// <summary>
        /// Ghi chú trong buổi tư vấn
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Khuyến nghị cho phụ huynh sau tư vấn
        /// </summary>
        public string? Recommendations { get; set; }
        /// <summary>
        /// Nếu buổi tư vấn sinh ra do phản ứng sau tiêm, liên kết tới VaccinationRecords
        /// </summary>
        public Guid? VaccinationRecordId { get; set; }

        [ForeignKey(nameof(VaccinationRecordId))]
        public virtual VaccinationRecord VaccinationRecord { get; set; }
    }
}
