using BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.CounselingAppointmentDTOs.Responds
{
    public class CounselingAppointmentRespondDTO
    {
        public Guid id { get; set; }
        public Guid StudentId { get; set; }

        public Guid ParentId { get; set; }

        public Guid StaffUserId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public int Duration { get; set; } 
        public ScheduleStatus status { get; set; }

        public string? Purpose { get; set; }

        public string? Notes { get; set; }

        public string? Recommendations { get; set; }

        public Guid? CheckupRecordId { get; set; }

        public Guid? VaccinationRecordId { get; set; }
    }
}
