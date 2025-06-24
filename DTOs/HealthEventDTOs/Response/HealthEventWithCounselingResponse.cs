using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;

namespace DTOs.HealthEventDTOs.Response
{
    public class HealthEventWithCounselingResponse
    {
        public Guid HealthEventId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public Guid? VaccinationRecordId { get; set; }
        public EventCategory EventCategory { get; set; }
        public EventType EventType { get; set; }
        public string Description { get; set; }
        public DateTime OccurredAt { get; set; }
        public EventStatus EventStatus { get; set; }
        public Guid ReportedUserId { get; set; }
        public string ReportedUserName { get; set; }

        public Guid? CounselingAppointmentId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public int? Duration { get; set; }
        public string? Purpose { get; set; }
        public Guid? StaffUserId { get; set; }
        public string? StaffUserName { get; set; }
    }
}
