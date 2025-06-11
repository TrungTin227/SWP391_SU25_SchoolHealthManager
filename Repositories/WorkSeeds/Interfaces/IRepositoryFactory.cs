namespace Repositories.WorkSeeds.Interfaces
{
    public interface IRepositoryFactory
    {
        IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class;

        TRepository GetCustomRepository<TRepository>()
            where TRepository : class;
    }
}