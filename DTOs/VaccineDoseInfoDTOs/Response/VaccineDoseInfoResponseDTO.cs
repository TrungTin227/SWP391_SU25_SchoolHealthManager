namespace DTOs.VaccineDoseInfoDTOs.Response
{
    public class VaccineDoseInfoResponseDTO
    {
        public Guid Id { get; set; }
        public Guid VaccineTypeId { get; set; }
        public string VaccineTypeName { get; set; } = string.Empty;
        public string VaccineTypeCode { get; set; } = string.Empty;
        public int DoseNumber { get; set; }
        public int RecommendedAgeMonths { get; set; }
        public int MinIntervalDays { get; set; }
        public Guid? PreviousDoseId { get; set; }
        public string? PreviousDoseName { get; set; }
        public int TotalNextDoses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}