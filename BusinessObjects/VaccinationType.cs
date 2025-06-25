using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class VaccinationType : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(30)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Group { get; set; }

        public bool IsActive { get; set; }

        // Thời điểm khuyến nghị theo tháng tuổi
        public int RecommendedAgeMonths { get; set; }

        // Khoảng cách tối thiểu giữa hai mũi (ngày)
        public int MinIntervalDays { get; set; }

        // Một loại vắc-xin có nhiều thông tin liều
        public virtual ICollection<VaccineDoseInfo> VaccineDoseInfos { get; set; }
            = new List<VaccineDoseInfo>();

        // Một loại vắc-xin cũng có thể được lên lịch nhiều lần
        public virtual ICollection<VaccinationSchedule> Schedules { get; set; }
            = new List<VaccinationSchedule>();
        public virtual ICollection<MedicationLot> MedicationLots { get; set; } = new List<MedicationLot>();

    }
}
