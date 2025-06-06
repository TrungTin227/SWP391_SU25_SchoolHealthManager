namespace Repositories.WorkSeeds.Interfaces
{
    public interface IRepositoryFactory // Phải là interface, không phải class
    {
        IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class;

        TRepository GetCustomRepository<TRepository>()
            where TRepository : class;
    }
}