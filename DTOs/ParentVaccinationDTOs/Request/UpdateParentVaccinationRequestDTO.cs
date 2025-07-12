using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentVaccinationDTOs.Request
{
    public class UpdateParentVaccinationRequestDTO
    {
        public Guid Id { get; set; }
        public DateTime? AdministeredAt { get; set; }
        public int? DoseNumber { get; set; }
    }

}
