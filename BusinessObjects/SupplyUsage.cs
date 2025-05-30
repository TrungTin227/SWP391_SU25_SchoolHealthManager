using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class SupplyUsage : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid HealthEventId { get; set; }

        [ForeignKey(nameof(HealthEventId))]
        public virtual HealthEvent HealthEvent { get; set; }

        [Required]
        public Guid MedicalSupplyId { get; set; }

        [ForeignKey(nameof(MedicalSupplyId))]
        public virtual MedicalSupply? MedicalSupply { get; set; }

        [Required]
        public int QuantityUsed { get; set; }

        [Required]
        public Guid NurseProfileId { get; set; }

        [ForeignKey(nameof(NurseProfileId))]
        public virtual NurseProfile? UsedByNurse { get; set; }

        [MaxLength(200)]
        public string Notes { get; set; }
    }
}
