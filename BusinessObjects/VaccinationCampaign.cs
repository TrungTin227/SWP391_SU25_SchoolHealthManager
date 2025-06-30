using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class VaccinationCampaign : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(9)]
        public string? SchoolYear { get; set; }
        public string Description { get; set; }
        public VaccinationCampaignStatus Status { get; set; } = VaccinationCampaignStatus.Pending;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual ICollection<VaccinationSchedule> Schedules { get; set; }
            = new List<VaccinationSchedule>();
    }
}
