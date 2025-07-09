using BusinessObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentVaccinationDTOs.Request
{
    public class CreateParentVaccinationRequestDTO
    {

        public Guid StudentId { get; set; }
        public Guid VaccineTypeId { get; set; }
        public int DoseNumber { get; set; }          // liều 1, 2…
        public DateTime AdministeredAt { get; set; } // ngày tiêm tại nhà
    }
}
