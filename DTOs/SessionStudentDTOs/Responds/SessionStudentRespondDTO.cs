using BusinessObjects;
using BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SessionStudentDTOs.Responds
{
    public class SessionStudentRespondDTO
    {
        public Guid SessionStudentId { get; set; }
        public Guid VaccinationScheduleId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentAttendStatus { get; set; } // Present, Absent, Excused, etc.
        public DateTime? CheckInTime { get; set; }
        public string? Notes { get; set; }
        public string ConsentStatus { get; set; }

        // Chữ ký số hoặc confirmation token từ phụ huynh
        public string? ParentSignature { get; set; }

        // Ghi chú từ phụ huynh khi ký (lý do từ chối, yêu cầu đặc biệt...)
        public string? ParentNotes { get; set; }
        public DateTime? ParentNotifiedAt { get; set; }

        // Thời gian phụ huynh ký (đồng ý hoặc từ chối)
        public DateTime? ParentSignedAt { get; set; }
        public DateTime? ConsentDeadline { get; set; }

    }
}
