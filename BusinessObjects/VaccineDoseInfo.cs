using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    //Nếu có nhiều mũi tiêm cho 1 loại vắc xin thì tạo bảng này
    public class VaccineDoseInfo
    {
        [Key]
        public Guid VaccineTypeId { get; set; }
        public int DoseNumber { get; set; }    // Liều 1, 2, 3…
        public int RecommendedAgeMonths { get; set; }
        public int MinIntervalDays { get; set; }
        public VaccineType VaccineType { get; set; }
        public ICollection<VaccineDoseInfo> NextDoseInfos { get; set; } // các lần liều tiếp theo
    }
}
