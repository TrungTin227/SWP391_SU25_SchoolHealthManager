using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SessionStudentDTOs.Requests
{
    public class UpdateSessionStudentCheckInRequest
    {
        [Required(ErrorMessage = "Cần nhập ít nhất 1 Sesion Student Id")]
        public List<Guid> SessionStudentId { get; set; }
        public string? Note { get; set; }
    }
}
