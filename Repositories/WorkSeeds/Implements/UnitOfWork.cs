using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Repositories.WorkSeeds.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SchoolHealthManagerDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IMedicationRepository _medicationRepository;
        private readonly IMedicationLotRepository _medicationLotRepository;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed;

        public UnitOfWork(SchoolHealthManagerDbContext context,
                         IUserRepository userRepository,
                         IMedicationRepository medicationRepository,
                         IMedicationLotRepository medicationLotRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _medicationRepository = medicationRepository ?? throw new ArgumentNullException(nameof(medicationRepository));
            _medicationLotRepository = medicationLotRepository ?? throw new ArgumentNullException(nameof(medicationLotRepository));
        }

        public IUserRepository UserRepository => _userRepository;
        public IMedicationRepository MedicationRepository => _medicationRepository;
        public IMedicationLotRepository MedicationLotRepository => _medicationLotRepository;

        // Property to check if there's an active transaction
        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Implementation of BeginTransactionAsync
        public async Task<IDbContextTransaction> BeginTransactionAsync(
           IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
           CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already active. Only one transaction can be active at a time.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            try
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                return; // No transaction to rollback
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                // Rollback any active transaction
                if (_currentTransaction != null)
                {
                    await RollbackTransactionAsync();
                }
                await _context.DisposeAsync();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        // Implement IDisposable for synchronous disposal
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Rollback any active transaction synchronously
                if (_currentTransaction != null)
                {
                    try
                    {
                        _currentTransaction.Rollback();
                    }
                    catch
                    {
                        // Ignore rollback exceptions during disposal
                    }
                    finally
                    {
                        _currentTransaction.Dispose();
                        _currentTransaction = null;
                    }
                }
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}