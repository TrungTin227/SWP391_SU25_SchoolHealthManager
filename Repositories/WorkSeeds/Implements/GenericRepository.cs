using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.WorkSeeds
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            // Apply AsNoTracking for better read performance
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (predicate is not null)
                query = query.Where(predicate);

            if (includes?.Any() == true)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            // Use FindAsync if no includes for efficiency
            if (includes == null || includes.Length == 0)
                return await _dbSet.FindAsync(id);

            // Build query with includes
            var query = _dbSet.AsNoTracking().AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Dynamically build a predicate for the entity key
            var key = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();
            if (key == null || key.Properties.Count != 1)
                throw new InvalidOperationException("Entity không có khóa chính duy nhất được hỗ trợ.");

            var keyProperty = key.Properties.First();
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var propertyAccess = Expression.Property(parameter, keyProperty.Name);
            var equalsExpression = Expression.Equal(propertyAccess, Expression.Constant(id));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equalsExpression, parameter);

            return await query.FirstOrDefaultAsync(lambda);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
    }
}
