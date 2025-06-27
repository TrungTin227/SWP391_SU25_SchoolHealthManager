using DTOs.CounselingAppointmentDTOs.Responds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers.Mappers
{
    public static class CounselingAppointmentMappings
    {
        public static CounselingAppointmentRespondDTO MapToCounselingAppointmentResponseDTO(CounselingAppointment appointment)
        {
            return new CounselingAppointmentRespondDTO
            {
                id = appointment.Id,
                StudentId = appointment.StudentId,
                ParentId = appointment.ParentId,
                StaffUserId = appointment.StaffUserId,
                AppointmentDate = appointment.AppointmentDate,
                Duration = appointment.Duration,
                Purpose = appointment.Purpose,
                CheckupRecordId = appointment.CheckupRecordId,
                VaccinationRecordId = appointment.VaccinationRecordId,
                status = appointment.Status,
                Notes = appointment.Notes,
                Recommendations = appointment.Recommendations
            };
        }

    }
}
