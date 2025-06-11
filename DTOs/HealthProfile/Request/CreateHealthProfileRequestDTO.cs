using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.Common;

namespace DTOs.HealthProfile.Request
{
    public class CreateHealthProfileRequestDTO
    {
        [Required(ErrorMessage = "Mã học sinh là bắt buộc"), MaxLength(20)]

        public Guid StudentId { get; set; }
        [Required(ErrorMessage = "Mã phụ huynh là bắt buộc"), MaxLength(20)]

        public Guid ParentId { get; set; }
        [Required(ErrorMessage = "Version là bắt buộc"), MaxLength(30)]

        public int Version { get; set; }
        public DateTime ProfileDate { get; set; }
        public string Allergies { get; set; }
        public string ChronicConditions { get; set; }
        public string TreatmentHistory { get; set; }
        [Required(ErrorMessage = "Vision Level là bắt buộc"), MaxLength(50)]

        public VisionLevel Vision { get; set; }
        [Required(ErrorMessage = "Hearing Level là bắt buộc"), MaxLength(50)]

        public HearingLevel Hearing { get; set; }
        public string VaccinationSummary { get; set; }
    }
}
