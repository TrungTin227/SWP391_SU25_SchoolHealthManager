using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.WorkSeeds.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SchoolHealthManagerDbContext _context;
        private readonly IUserRepository _userRepository;
        private bool _disposed;

        public UnitOfWork(SchoolHealthManagerDbContext context, IUserRepository userRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public IUserRepository UserRepository => _userRepository;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Triển khai BeginTransactionAsync
        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            // Gọi DatabaseFacade.BeginTransactionAsync với IsolationLevel
            return await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }
        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
