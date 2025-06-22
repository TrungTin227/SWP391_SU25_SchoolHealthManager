using BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SessionStudentDTOs.Requests
{
    public class ParentAcptVaccine
    {
        public Guid StudentId { get; set; }
        public Guid VaccinationScheduleId { get; set; }

        public string? ParentNote { get; set; }
        public string ParentSignature { get; set; }
        public ParentConsentStatus ConsentStatus { get; set; }

    }
}
