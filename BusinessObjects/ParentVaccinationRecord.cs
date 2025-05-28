using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class ParentVaccinationRecord : BaseEntity
    {
        [Key] public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        public Guid VaccineTypeId { get; set; }
        public VaccineType VaccineType { get; set; }

        public int DoseNumber { get; set; }          // liều 1, 2…
        public DateTime AdministeredAt { get; set; } // ngày tiêm tại nhà

        public Guid ParentUserId { get; set; }
        public Parent Parent { get; set; }
    }
}
