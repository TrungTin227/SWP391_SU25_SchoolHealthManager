using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BusinessObjects
{
    public class VaccinationRecord : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        // Liên kết với session (chứa thông tin consent)
        public Guid SessionStudentId { get; set; }
        [ForeignKey(nameof(SessionStudentId))]
        public virtual SessionStudent SessionStudent { get; set; }

        // Lô vắc-xin sử dụng
        public Guid VaccineLotId { get; set; }
        [ForeignKey(nameof(VaccineLotId))]
        public virtual MedicationLot VaccineLot { get; set; }

        public DateTime AdministeredDate { get; set; }

        // Ai thực hiện tiêm
        public Guid VaccinatedById { get; set; }
        [ForeignKey(nameof(VaccinatedById))]
        public virtual User VaccinatedBy { get; set; }

        public DateTime VaccinatedAt { get; set; }
        public bool ReactionFollowup24h { get; set; }
        public bool ReactionFollowup72h { get; set; }
        public VaccinationReactionSeverity ReactionSeverity { get; set; }

        public virtual ICollection<CounselingAppointment> CounselingAppointments { get; set; }
            = new List<CounselingAppointment>();
        // Computed properties để truy cập dễ dàng

        public virtual ICollection<HealthEvent> HealthEvents { get; set; } = new List<HealthEvent>();

        [NotMapped]
        public Student Student => SessionStudent?.Student;

        [NotMapped]
        public VaccinationSchedule Schedule => SessionStudent?.VaccinationSchedule;

    }
}