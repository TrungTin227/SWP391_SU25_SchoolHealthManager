namespace Repositories.WorkSeeds.Interfaces
{

    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IStudentRepository StudentRepository { get; }
        IParentRepository ParentRepository { get; }
        IParentMedicationDeliveryRepository ParentMedicationDeliveryRepository { get; }
        IMedicationRepository MedicationRepository { get; }
        IMedicationLotRepository MedicationLotRepository { get; }
        IMedicalSupplyRepository MedicalSupplyRepository { get; }
        IMedicalSupplyLotRepository MedicalSupplyLotRepository { get; }
        IHealthEventRepository HealthEventRepository { get; }
    }
}
