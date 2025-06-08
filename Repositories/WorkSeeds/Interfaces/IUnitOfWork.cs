namespace Repositories.WorkSeeds.Interfaces
{

    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IStudentRepository StudentRepository { get; }

        IParentRepository ParentRepository { get; }
        IMedicationRepository MedicationRepository { get; }
        IMedicationLotRepository MedicationLotRepository { get; }
    }
}
