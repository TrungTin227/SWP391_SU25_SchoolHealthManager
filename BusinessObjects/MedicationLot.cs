using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class MedicationLot : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public Medication Medication { get; set; }
        [MaxLength(100)]
        public string LotNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        [MaxLength(100)]
        public string StorageLocation { get; set; }
        public ICollection<Dispense> Dispenses { get; set; }
        public ICollection<EventMedication> EventMedications { get; set; }
        public ICollection<VaccinationRecord> VaccinationRecords { get; set; }
    }
}
