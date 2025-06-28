using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SessionStudentDTOs.Requests
{
    public class GetSessionStudentsRequest
    {
        public Guid? StudentId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? VaccinationScheduleId { get; set; } // optional filter nếu cần thêm
    }
}
