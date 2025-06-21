using System.Linq.Expressions;
namespace Repositories.WorkSeeds.Interfaces
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        // Basic CRUD
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(TKey id);
        Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);

        // Batch operations
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateRangeAsync(IEnumerable<TEntity> entities);
        Task DeleteRangeAsync(IEnumerable<TKey> ids);
        Task<List<TEntity>> GetByIdsAsync(List<TKey> ids);


        // Query operations

        IQueryable<TEntity> GetQueryable();

        Task<IReadOnlyList<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);

        Task<PagedList<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

        // Soft delete support (nếu entity có IsDeleted property)
        Task<bool> SoftDeleteAsync(TKey id, Guid? deletedBy = null);
        Task<bool> RestoreAsync(TKey id, Guid? restoredBy = null);
    }
}
