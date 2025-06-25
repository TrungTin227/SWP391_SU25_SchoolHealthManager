using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Repositories.WorkSeeds.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SchoolHealthManagerDbContext _context;
        private readonly IRepositoryFactory _repositoryFactory;
        private IDbContextTransaction? _transaction;

        // Specific repositories
        private IUserRepository? _userRepository;
        private IHealProfileRepository? _healProfileRepository;
        private IStudentRepository? _studentRepository;
        private ISessionStudentRepository? _sessionStudentRepository;
        private IParentRepository? _parentRepository;
        private IParentMedicationDeliveryRepository? _parentMedicationDeliveryRepository;
        private IMedicationRepository? _medicationRepository;
        private IMedicationLotRepository? _medicationLotRepository;
        private IMedicalSupplyRepository? _medicalSupplyRepository;
        private IMedicalSupplyLotRepository? _medicalSupplyLotRepository;
        private IHealthEventRepository? _healthEventRepository;
        private IVaccineDoseInfoRepository? _vaccineDoseInfoRepository;
        private IVaccineTypeRepository? _vaccineTypeRepository;
        private IVaccineLotRepository? _vaccineLotRepository;
        private IVaccinationCampaignRepository? _vaccinationCampaignRepository;
        private IVaccinationScheduleRepository? _vaccinationScheduleRepository;
        private IParentVaccinationRepository? _parentVaccinationRepository;
        private ICheckupCampaignRepository? _checkupCampaignRepository;
        private ICheckupScheduleRepository? _checkupScheduleRepository;

        public UnitOfWork(SchoolHealthManagerDbContext context, IRepositoryFactory repositoryFactory)
        {
            _context = context;
            _repositoryFactory = repositoryFactory;
        }

        public IUserRepository UserRepository =>
            _userRepository ??= _repositoryFactory.GetCustomRepository<IUserRepository>();
        public IHealProfileRepository HealProfileRepository => 
            _healProfileRepository ??= _repositoryFactory.GetCustomRepository<IHealProfileRepository>();
        public IStudentRepository StudentRepository =>
            _studentRepository ??= _repositoryFactory.GetCustomRepository<IStudentRepository>();
        public ISessionStudentRepository SessionStudentRepository => 
            _sessionStudentRepository ??= _repositoryFactory.GetCustomRepository<ISessionStudentRepository>();
        public IParentRepository ParentRepository =>
            _parentRepository ??= _repositoryFactory.GetCustomRepository<IParentRepository>();
        public IParentMedicationDeliveryRepository ParentMedicationDeliveryRepository => 
            _parentMedicationDeliveryRepository ??= _repositoryFactory.GetCustomRepository<IParentMedicationDeliveryRepository>();
        public IMedicationRepository MedicationRepository =>
            _medicationRepository ??= _repositoryFactory.GetCustomRepository<IMedicationRepository>();

        public IMedicationLotRepository MedicationLotRepository =>
            _medicationLotRepository ??= _repositoryFactory.GetCustomRepository<IMedicationLotRepository>();

        public IMedicalSupplyRepository MedicalSupplyRepository =>
            _medicalSupplyRepository ??= _repositoryFactory.GetCustomRepository<IMedicalSupplyRepository>();
        public IMedicalSupplyLotRepository MedicalSupplyLotRepository =>
            _medicalSupplyLotRepository ??= _repositoryFactory.GetCustomRepository<IMedicalSupplyLotRepository>();
        public IHealthEventRepository HealthEventRepository =>
            _healthEventRepository ??= _repositoryFactory.GetCustomRepository<IHealthEventRepository>();
        public IVaccineDoseInfoRepository VaccineDoseInfoRepository => 
            _vaccineDoseInfoRepository ??= _repositoryFactory.GetCustomRepository<IVaccineDoseInfoRepository>();
        public IVaccineTypeRepository VaccineTypeRepository => 
            _vaccineTypeRepository ??= _repositoryFactory.GetCustomRepository<IVaccineTypeRepository>();
        public IVaccineLotRepository VaccineLotRepository => 
            _vaccineLotRepository ??= _repositoryFactory.GetCustomRepository<IVaccineLotRepository>();
        public IVaccinationCampaignRepository VaccinationCampaignRepository => 
            _vaccinationCampaignRepository ??= _repositoryFactory.GetCustomRepository<IVaccinationCampaignRepository>();
        public IVaccinationScheduleRepository VaccinationScheduleRepository => 
            _vaccinationScheduleRepository ??= _repositoryFactory.GetCustomRepository<IVaccinationScheduleRepository>();
        public IParentVaccinationRepository ParentVaccinationRepository => 
            _parentVaccinationRepository ??= _repositoryFactory.GetCustomRepository<IParentVaccinationRepository>();
        public ICheckupCampaignRepository CheckupCampaignRepository => 
            _checkupCampaignRepository ??= _repositoryFactory.GetCustomRepository<ICheckupCampaignRepository>();
        public ICheckupScheduleRepository CheckupScheduleRepository => 
            _checkupScheduleRepository ??= _repositoryFactory.GetCustomRepository<ICheckupScheduleRepository>();
        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class
        {
            return _repositoryFactory.GetRepository<TEntity, TKey>();
        }

        public bool HasActiveTransaction => _transaction != null;

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already active.");

            _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to rollback.");

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();

            if (_context != null)
                await _context.DisposeAsync();
        }
    }
}