using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.VaccinationRecordDTOs.Request
{
    public class UpdateVaccinationRecordRequest
    {
        public Guid Id { get; set; }
        public DateTime? AdministeredDate { get; set; }
        public DateTime? VaccinatedAt { get; set; }
        public bool? ReactionFollowup24h { get; set; }
        public bool? ReactionFollowup72h { get; set; }
        public int? ReactionSeverity { get; set; }
    }
}
