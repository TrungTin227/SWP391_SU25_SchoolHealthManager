using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class VaccineType : BaseEntity
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

        // thông tin về tháng tuổi tiêm mũi nào và khoảng cách giữa các mũi
        public int RecommendedAgeMonths { get; set; }   // Ví dụ: 0, 2, 6, 12, 60
        public int MinIntervalDays { get; set; }   // Khoảng cách tối thiểu (ngày)
    }
}
