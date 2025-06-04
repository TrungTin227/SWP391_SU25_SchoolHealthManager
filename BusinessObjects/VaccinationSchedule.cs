using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class VaccinationSchedule : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public Guid CampaignId { get; set; }
        public VaccinationCampaign Campaign { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public DateTime ScheduledAt { get; set; }
        public ScheduleStatus ScheduleStatus { get; set; }
    }
}
