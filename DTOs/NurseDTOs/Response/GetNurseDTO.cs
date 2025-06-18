using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.NurseDTOs.Response
{
    public class GetNurseDTO
    {
        public Guid UserID { get; set; }
        public string? Username { get; set; }

    }
}
