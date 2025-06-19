namespace DTOs.VaccinationScheduleDTOs.Response
{
    public class VaccinationRecordSummaryDTO
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime AdministeredDate { get; set; }
        public string VaccinatedByName { get; set; } = string.Empty;
        public bool ReactionFollowup24h { get; set; }
        public bool ReactionFollowup72h { get; set; }
    }
}
