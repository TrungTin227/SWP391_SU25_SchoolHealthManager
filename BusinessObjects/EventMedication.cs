using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class EventMedication : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid HealthEventId { get; set; }
        public HealthEvent HealthEvent { get; set; }
        public Guid MedicationLotId { get; set; }
        public MedicationLot MedicationLot { get; set; }
        public int Quantity { get; set; }
    }
}
