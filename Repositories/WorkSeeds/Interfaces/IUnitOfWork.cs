using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.WorkSeeds.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Repository cho người dùng
        IUserRepository UserRepository { get; }

        // phương thức BeginTransaction
        Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default);
    }
}
