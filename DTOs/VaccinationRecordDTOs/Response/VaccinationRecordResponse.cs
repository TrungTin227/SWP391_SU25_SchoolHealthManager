using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.VaccinationRecordDTOs.Response
{
    public class VaccinationRecordResponse
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public Guid StudentId { get; set; }
        public Guid VaccineTypeId { get; set; }
        public Guid VaccineLotId { get; set; }
        public DateTime AdministeredDate { get; set; }
        public bool ConsentSigned { get; set; }
        public Guid VaccinatedBy { get; set; }
        public DateTime VaccinatedAt { get; set; }
        public bool ReactionFollowup24h { get; set; }
        public bool ReactionFollowup72h { get; set; }
    }
}
