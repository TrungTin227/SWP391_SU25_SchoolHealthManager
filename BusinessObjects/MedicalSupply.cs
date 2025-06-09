using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class MedicalSupply : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required, MaxLength(50)]
        public string Unit { get; set; } = ""; // e.g., "pcs", "box", "bottle"
        public int CurrentStock { get; set; } 
        public int MinimumStock { get; set; }

        public bool IsActive { get; set; }

        // Navigation
        public virtual ICollection<MedicalSupplyLot> Lots { get; set; } = new List<MedicalSupplyLot>();
   
    }
}
