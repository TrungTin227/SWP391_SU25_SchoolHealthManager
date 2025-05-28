using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class HealthProfile : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public Guid ParentId { get; set; }
        public Parent Parent { get; set; }
        public int Version { get; set; }
        public DateTime ProfileDate { get; set; }
        public string Allergies { get; set; }
        public string ChronicConditions { get; set; }
        public string TreatmentHistory { get; set; }
        [MaxLength(50)]
        public string Vision { get; set; }
        [MaxLength(50)]
        public string Hearing { get; set; }
        public string VaccinationSummary { get; set; }
    }
}
