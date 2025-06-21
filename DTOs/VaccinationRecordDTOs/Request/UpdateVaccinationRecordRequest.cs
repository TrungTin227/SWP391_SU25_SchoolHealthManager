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
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid VaccineTypeId { get; set; }

        [Required]
        public Guid VaccineLotId { get; set; }

        [Required]
        public DateTime AdministeredDate { get; set; }

        public bool ConsentSigned { get; set; } = false;

        [Required]
        public Guid VaccinatedBy { get; set; }

        [Required]
        public DateTime VaccinatedAt { get; set; }

        public bool ReactionFollowup24h { get; set; } = false;

        public bool ReactionFollowup72h { get; set; } = false;

    }
}
