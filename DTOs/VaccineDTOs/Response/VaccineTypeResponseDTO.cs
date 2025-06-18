namespace DTOs.VaccineDTOs.Response
{
    public class VaccineTypeResponseDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int RecommendedAgeMonths { get; set; }
        public int MinIntervalDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int TotalDoses { get; set; }
        public int TotalSchedules { get; set; }
        public int TotalMedicationLots { get; set; }
    }
}