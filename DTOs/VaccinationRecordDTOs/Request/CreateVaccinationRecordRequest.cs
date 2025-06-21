using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.VaccinationRecordDTOs.Request
{
    public class CreateVaccinationRecordRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required(ErrorMessage = "ID học sinh là bắt buộc!")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "ID của vaccine là bắt buộc!")]
        public Guid VaccineTypeId { get; set; }

        [Required]
        public Guid VaccineLotId { get; set; }

        [Required]
        public DateTime AdministeredDate { get; set; }

        public bool ConsentSigned { get; set; } = false;

        [Required(ErrorMessage = "ID của người tiêm là bắt buộc!")]
        public Guid VaccinatedBy { get; set; }

        [Required]
        public DateTime VaccinatedAt { get; set; }

        public bool ReactionFollowup24h { get; set; } = false;

        public bool ReactionFollowup72h { get; set; } = false;

    }
}
