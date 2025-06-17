using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class VaccineDoseInfo
    {
        [Key]
        public Guid Id { get; set; }

        // FK về loại vắc-xin
        public Guid VaccineTypeId { get; set; }
        [ForeignKey(nameof(VaccineTypeId))]
        public virtual VaccinationType VaccineType { get; set; }

        public int DoseNumber { get; set; }
        public int RecommendedAgeMonths { get; set; }
        public int MinIntervalDays { get; set; }

        // Tự tham chiếu: mũi trước
        public Guid? PreviousDoseId { get; set; }
        [ForeignKey(nameof(PreviousDoseId))]
        public virtual VaccineDoseInfo PreviousDose { get; set; }

        // Các mũi kế tiếp
        public virtual ICollection<VaccineDoseInfo> NextDoses { get; set; }
            = new List<VaccineDoseInfo>();
    }
}