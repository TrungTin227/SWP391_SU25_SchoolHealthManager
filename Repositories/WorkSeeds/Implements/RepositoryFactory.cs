using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Repositories.WorkSeeds.Implements
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly SchoolHealthManagerDbContext _context;
        private readonly IServiceProvider _serviceProvider; // Thêm field này
        private readonly Dictionary<Type, object> _repositories = new();

        public RepositoryFactory(SchoolHealthManagerDbContext context, IServiceProvider serviceProvider) // Thêm parameter
        {
            _context = context;
            _serviceProvider = serviceProvider; // Gán giá trị
        }

        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class
        {
            var type = typeof(TEntity);
            if (_repositories.ContainsKey(type))
            {
                return (IGenericRepository<TEntity, TKey>)_repositories[type];
            }

            var repository = new GenericRepository<TEntity, TKey>(_context);
            _repositories.Add(type, repository);
            return repository;
        }

        public TRepository GetCustomRepository<TRepository>()
            where TRepository : class
        {
            var type = typeof(TRepository);
            if (_repositories.ContainsKey(type))
            {
                return (TRepository)_repositories[type];
            }

            // Use DI to resolve custom repository
            var repository = _serviceProvider.GetRequiredService<TRepository>();
            _repositories.Add(type, repository);
            return repository;
        }
    }
}
