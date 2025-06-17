using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class MedicationLot : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? MedicationId { get; set; }
        public virtual Medication Medication { get; set; }
        public Guid? VaccineTypeId { get; set; }
        public virtual VaccinationType VaccineType { get; set; }
        public LotType Type { get; set; }
        [MaxLength(100)]
        public string LotNumber { get; set; }

        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }

        [MaxLength(100)]
        public string StorageLocation { get; set; }

        public virtual ICollection<Dispense> Dispenses { get; set; }
            = new List<Dispense>();

        public virtual ICollection<EventMedication> EventMedications { get; set; }
            = new List<EventMedication>();

        // Một lô thuốc có thể dùng cho nhiều bản ghi tiêm
        public virtual ICollection<VaccinationRecord> VaccinationRecords { get; set; }
            = new List<VaccinationRecord>();
    }

}
