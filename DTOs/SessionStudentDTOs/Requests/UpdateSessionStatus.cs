using BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SessionStudentDTOs.Requests
{
    public class UpdateSessionStatus
    {
        public List<Guid> SessionStudentIds { get; set; } 
        public SessionStatus Status { get; set; } 
       
    }
}
