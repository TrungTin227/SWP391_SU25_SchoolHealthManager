using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class Student : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(20)]
        public string StudentCode { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{LastName} {FirstName}".Trim();

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Grade { get; set; }

        [MaxLength(10)]
        public string? Section { get; set; }

        [MaxLength(150)]
        public string? Image { get; set; }

        public Guid ParentUserId { get; set; }
        [ForeignKey(nameof(ParentUserId))]
        public Parent Parent { get; set; }

        public ICollection<HealthProfile> HealthProfiles { get; set; } = new List<HealthProfile>();
        public ICollection<HealthEvent> HealthEvents { get; set; } = new List<HealthEvent>();
        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
        public virtual ICollection<CounselingAppointment> CounselingAppointments { get; set; } = new List<CounselingAppointment>();
        public virtual ICollection<SessionStudent> SessionStudents { get; set; } = new List<SessionStudent>();
        
    }
}