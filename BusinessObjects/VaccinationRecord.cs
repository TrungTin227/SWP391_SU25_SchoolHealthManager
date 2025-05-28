using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class VaccinationRecord : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public VaccinationCampaign Campaign { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public Guid VaccineTypeId { get; set; }
        public VaccineType VaccineType { get; set; }
        public Guid VaccineLotId { get; set; }
        public MedicationLot VaccineLot { get; set; }
        public DateTime AdministeredDate { get; set; }
        public bool ConsentSigned { get; set; }
        public Guid VaccinatedBy { get; set; }
        public User VaccinatedUser { get; set; }
        public DateTime VaccinatedAt { get; set; }
        public bool ReactionFollowup24h { get; set; }
        public bool ReactionFollowup72h { get; set; }
    }
}
