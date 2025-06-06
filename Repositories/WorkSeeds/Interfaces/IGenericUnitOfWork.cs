using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Repositories.WorkSeeds.Interfaces
{
    public interface IGenericUnitOfWork : IAsyncDisposable, IDisposable
    {
        // Generic repository access
        IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class;

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
