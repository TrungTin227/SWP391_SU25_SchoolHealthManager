using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Medication : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Unit { get; set; }
        [MaxLength(100)]
        public string DosageForm { get; set; }
        public ICollection<MedicationLot> Lots { get; set; }
    }

}
