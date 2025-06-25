namespace DTOs.VaccinationScheduleDTOs.Request
{
    public class UpdateVaccinationScheduleRequest
    {
        public Guid? VaccinationTypeId { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public List<Guid>? StudentIds { get; set; }

        public string? Notes { get; set; }
    }
}