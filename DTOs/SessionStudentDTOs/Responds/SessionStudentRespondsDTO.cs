using BusinessObjects;
using BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SessionStudentDTOs.Responds
{
    public class SessionStudentRespondsDTO
    {
        public Guid VaccinationScheduleId { get; set; }
        public Guid StudentId { get; set; }
        public SessionStatus Status { get; set; } // Present, Absent, Excused, etc.

        public DateTime? CheckInTime { get; set; }
        public string? Notes { get; set; }

        // Thời gian gửi thông báo cho phụ huynh
        public DateTime? ParentNotifiedAt { get; set; }

        // Thời gian phụ huynh ký (đồng ý hoặc từ chối)
        public DateTime? ParentSignedAt { get; set; }

        // Trạng thái đồng ý của phụ huynh
        public ParentConsentStatus ConsentStatus { get; set; } 

        // Chữ ký số hoặc confirmation token từ phụ huynh
        public string? ParentSignature { get; set; }

        // Ghi chú từ phụ huynh khi ký (lý do từ chối, yêu cầu đặc biệt...)
        public string? ParentNotes { get; set; }

        // Deadline để phụ huynh ký (có thể tính từ ScheduledAt - X ngày)
        public DateTime? ConsentDeadline { get; set; }

    }
}
