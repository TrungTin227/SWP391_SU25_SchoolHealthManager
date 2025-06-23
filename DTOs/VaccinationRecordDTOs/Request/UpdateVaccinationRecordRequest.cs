using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.VaccinationRecordDTOs.Request
{
    public class UpdateVaccinationRecordRequest
    {
        public DateTime? AdministeredDate { get; set; }
        public bool? ConsentSigned { get; set; }
        public Guid? VaccineLotId { get; set; }
        public Guid? VaccinatedBy { get; set; }
        public DateTime? VaccinatedAt { get; set; }
        public bool? ReactionFollowup24h { get; set; }
        public bool? ReactionFollowup72h { get; set; }

    }
}
