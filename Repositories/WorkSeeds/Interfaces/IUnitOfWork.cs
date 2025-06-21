namespace Repositories.WorkSeeds.Interfaces
{

    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IStudentRepository StudentRepository { get; }
        IHealProfileRepository HealProfileRepository { get; }
        ISessionStudentRepository SessionStudentRepository { get; }
        IParentRepository ParentRepository { get; }
        IParentMedicationDeliveryRepository ParentMedicationDeliveryRepository { get; }
        IMedicationRepository MedicationRepository { get; }
        IMedicationLotRepository MedicationLotRepository { get; }
        IMedicalSupplyRepository MedicalSupplyRepository { get; }
        IMedicalSupplyLotRepository MedicalSupplyLotRepository { get; }
        IHealthEventRepository HealthEventRepository { get; }
        IVaccineTypeRepository VaccineTypeRepository { get; }
        IVaccineDoseInfoRepository VaccineDoseInfoRepository { get; }
        IVaccineLotRepository VaccineLotRepository { get; }
        IVaccinationCampaignRepository VaccinationCampaignRepository { get; }
        IVaccinationScheduleRepository VaccinationScheduleRepository { get; }

    }
}
