using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class VaccinationSchedule : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string ScheduleType { get; set; }
        public Guid CampaignId { get; set; }
        public VaccinationCampaign Campaign { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string ScheduleStatus { get; set; }
    }
}
