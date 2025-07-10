using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class VaccinationSchedule : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public virtual VaccinationCampaign Campaign { get; set; }

        public Guid VaccinationTypeId { get; set; }
        [ForeignKey(nameof(VaccinationTypeId))]
        public virtual VaccinationType VaccinationType { get; set; }

        public DateTime ScheduledAt { get; set; }
        public ScheduleStatus ScheduleStatus { get; set; }

        // Quản lý học sinh thông qua bảng trung gian
        public virtual ICollection<SessionStudent> SessionStudents { get; set; } = new List<SessionStudent>();
    }
}