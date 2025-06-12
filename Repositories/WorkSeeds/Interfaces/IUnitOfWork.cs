namespace Repositories.WorkSeeds.Interfaces
{

    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IStudentRepository StudentRepository { get; }
        IHealProfileRepository HealProfileRepository { get; }
        IParentRepository ParentRepository { get; }
        IParentMedicationDeliveryRepository ParentMedicationDeliveryRepository { get; }
        IMedicationRepository MedicationRepository { get; }
        IMedicationLotRepository MedicationLotRepository { get; }
    }
}
