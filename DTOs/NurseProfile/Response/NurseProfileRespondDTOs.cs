using BusinessObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.NurseProfile.Response
{
    public class NurseProfileRespondDTOs
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }

        public string Position { get; set; }

        public string Department { get; set; }
    }
}
