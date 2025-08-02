using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.Common;
using DTOs.StudentDTOs.Response;

namespace DTOs.HealProfile.Responds
{
    public class HealProfileResponseDTO
    {
        public GetAllStudentDTO StudentInformation { get; set; }
        public Guid ProfileId { get; set; }
        public int Version { get; set; }
        public DateTime ProfileDate { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? TreatmentHistory { get; set; }
        public string? Vision { get; set; }
        public string? Hearing { get; set; }
        public string? VaccinationSummary { get; set; }
        public string? Gender { get; set; }
    }
}
