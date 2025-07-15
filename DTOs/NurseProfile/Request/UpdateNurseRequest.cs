using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.NurseProfile.Request
{
    public class UpdateNurseRequest
    {
        [Required]
        public Guid UserID { get; set; }

        [MaxLength(100)]
        public String? Position { get; set; }     // chức vụ (y tá, y tá trưởng) 

        [MaxLength(100)]
        public string? Department { get; set; }         //phòng ban (tiêm chủng, ...) 
    }
}
