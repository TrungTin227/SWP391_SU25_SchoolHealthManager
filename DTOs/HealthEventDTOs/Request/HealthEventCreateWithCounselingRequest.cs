using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;

namespace DTOs.HealthEventDTOs.Request
{
    public class HealthEventCreateWithCounselingRequest
    {
        public Guid StudentId { get; set; }
        public Guid? VaccinationRecordId { get; set; }
        public EventCategory EventCategory { get; set; }
        public EventType EventType { get; set; }
        public string Description { get; set; }
        public DateTime OccurredAt { get; set; }
        public EventStatus EventStatus { get; set; }
        public Guid ReportedUserId { get; set; }
        public DateTime? AppointmentDate { get; set; } // optional counseling appointment
        public int? Duration { get; set; }
        public string? Purpose { get; set; }
        public Guid? StaffUserId { get; set; }
    }
}
