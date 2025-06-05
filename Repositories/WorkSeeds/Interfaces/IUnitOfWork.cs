using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.WorkSeeds.Interfaces
{
    
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        // Repository properties
        IUserRepository UserRepository { get; }
        IMedicationRepository MedicationRepository { get; }
        IMedicationLotRepository MedicationLotRepository { get; }

        // Transaction management
        bool HasActiveTransaction { get; }

        Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        // Save changes
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
