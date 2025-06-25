using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.Common;

namespace DTOs.HealProfile.Requests
{
    public class CreateHealProfileRequestDTO
    {
        [Required(ErrorMessage = "Mã học sinh là bắt buộc")]
        public string StudentCode { get; set; }

        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? TreatmentHistory { get; set; }
        public VisionLevel? Vision { get; set; }
        public HearingLevel? Hearing { get; set; }
        public string? VaccinationSummary { get; set; }
        
    }
}
