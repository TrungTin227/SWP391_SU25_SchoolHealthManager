using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Dispense : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public Guid MedicationLotId { get; set; }
        public MedicationLot MedicationLot { get; set; }
        public int Quantity { get; set; }
        [MaxLength(200)]
        public string DispenseReason { get; set; }
        public Guid AdministeredBy { get; set; }
        public User AdministeredUser { get; set; }
        public DateTime AdministeredAt { get; set; }
    }
}
