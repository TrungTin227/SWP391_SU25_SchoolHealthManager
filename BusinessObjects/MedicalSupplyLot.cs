using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BusinessObjects 
{
    public class MedicalSupplyLot : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MedicalSupplyId { get; set; }

        [Required, MaxLength(50)]
        public string LotNumber { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        public DateTime ManufactureDate { get; set; }

        public int Quantity { get; set; }

        // Navigation
        [ForeignKey(nameof(MedicalSupplyId))]
        public virtual MedicalSupply MedicalSupply { get; set; }

        public virtual ICollection<SupplyUsage> SupplyUsages { get; set; } = new List<SupplyUsage>();
    }
}