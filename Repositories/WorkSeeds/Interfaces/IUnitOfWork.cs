namespace Repositories.WorkSeeds.Interfaces
{

    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IMedicationRepository MedicationRepository { get; }
        IMedicationLotRepository MedicationLotRepository { get; }
    }
}
