using DTOs.SessionStudentDTOs.Responds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers.Mappers
{
    public static class SessionStudentMappings
    {
        public static SessionStudentRespondDTO ToRespondDTO(this SessionStudent ss)
        {
            return new SessionStudentRespondDTO
            {
                SessionStudentId = ss.Id,
                VaccinationScheduleId = ss.VaccinationScheduleId,
                StudentId = ss.StudentId,
                StudentAttendStatus = ss.Status.ToString(),
                CheckInTime = ss.CheckInTime,
                Notes = ss.Notes,
                ConsentStatus = ss.ConsentStatus.ToString(),
                ParentSignature = ss.ParentSignature,
                ParentNotes = ss.ParentNotes,
                ParentNotifiedAt = ss.ParentNotifiedAt,
                ParentSignedAt = ss.ParentSignedAt,
                ConsentDeadline = ss.ConsentDeadline
            };
        }
    }
}
