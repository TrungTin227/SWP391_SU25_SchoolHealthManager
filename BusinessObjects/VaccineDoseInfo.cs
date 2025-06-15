using BusinessObjects;
using System.ComponentModel.DataAnnotations;

public class VaccineDoseInfo
{
    [Key]
    public Guid Id { get; set; } // Thêm primary key riêng

    public Guid VaccineTypeId { get; set; } // Foreign key
    public int DoseNumber { get; set; }
    public int RecommendedAgeMonths { get; set; }
    public int MinIntervalDays { get; set; }

    // Self-reference properties
    public Guid? PreviousDoseId { get; set; } // Foreign key cho liều trước

    // Navigation properties
    public VaccinationType VaccineType { get; set; }
    public VaccineDoseInfo PreviousDose { get; set; }
    public ICollection<VaccineDoseInfo> NextDoses { get; set; }
}