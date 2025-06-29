using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SessionStudentDTOs.Responds
{
    public class ParentAcptVaccineResult
    {
        public Guid StudentId { get; set; }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
