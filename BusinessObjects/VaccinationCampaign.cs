using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class VaccinationCampaign : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<VaccinationSchedule> Schedules { get; set; }
    }
}
