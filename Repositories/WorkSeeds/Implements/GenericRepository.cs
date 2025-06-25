using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.WorkSeeds.Implements
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
        where TEntity : class
    {
        protected readonly SchoolHealthManagerDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(SchoolHealthManagerDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>(); // Sửa lỗi syntax *dbSet = *context.Set<TEntity>()
        }

        // Repository chỉ làm việc với data, không set audit fields
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            return true;
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(id));
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TKey> ids)
        {
            var entities = await _dbSet.Where(e => ids.Contains(EF.Property<TKey>(e, "Id"))).ToListAsync();
            _dbSet.RemoveRange(entities);
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync();
        }

        public virtual async Task<PagedList<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (orderBy != null)
                query = orderBy(query);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<TEntity>(items, totalCount, pageNumber, pageSize);
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        // Soft delete nhận audit info từ bên ngoài
        public virtual async Task<bool> SoftDeleteAsync(TKey id, Guid? deletedBy = null)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;

            // Handle BaseEntity cũ
            if (IsInheritedFromBaseEntity(entity.GetType()))
            {
                SetProperty(entity, "IsDeleted", true);
                SetProperty(entity, "DeletedAt", DateTime.UtcNow);
                if (deletedBy.HasValue)
                {
                    SetProperty(entity, "DeletedBy", deletedBy.Value);
                    SetProperty(entity, "UpdatedAt", DateTime.UtcNow);
                    SetProperty(entity, "UpdatedBy", deletedBy.Value);
                }
            }
            else if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTime.UtcNow;
                if (deletedBy.HasValue)
                {
                    softDeletable.DeletedBy = deletedBy.Value;
                }

                if (entity is IBaseEntity<TKey> auditableEntity && deletedBy.HasValue)
                {
                    auditableEntity.UpdatedAt = DateTime.UtcNow;
                    auditableEntity.UpdatedBy = deletedBy.Value;
                }
            }
            else
            {
                _dbSet.Remove(entity);
                return true;
            }

            _dbSet.Update(entity);
            return true;
        }

        public virtual async Task<bool> RestoreAsync(TKey id, Guid? restoredBy = null)
        {
            var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(id));
            if (entity == null) return false;

            if (IsInheritedFromBaseEntity(entity.GetType()))
            {
                SetProperty(entity, "IsDeleted", false);
                SetProperty(entity, "DeletedAt", null);
                SetProperty(entity, "DeletedBy", null);
                if (restoredBy.HasValue)
                {
                    SetProperty(entity, "UpdatedAt", DateTime.UtcNow);
                    SetProperty(entity, "UpdatedBy", restoredBy.Value);
                }
            }
            else if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.IsDeleted = false;
                softDeletable.DeletedAt = null;
                softDeletable.DeletedBy = null;

                if (entity is IBaseEntity<TKey> auditableEntity && restoredBy.HasValue)
                {
                    auditableEntity.UpdatedAt = DateTime.UtcNow;
                    auditableEntity.UpdatedBy = restoredBy.Value;
                }
            }
            else
            {
                return false; // Entity không support soft delete
            }

            _dbSet.Update(entity);
            return true;
        }

        // Helper methods
        private bool IsInheritedFromBaseEntity(Type type)
        {
            return typeof(BusinessObjects.BaseEntity).IsAssignableFrom(type);
        }

        private void SetProperty(object obj, string propertyName, object? value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            property?.SetValue(obj, value);
        }
        public virtual IQueryable<TEntity> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<List<TEntity>> GetByIdsAsync(List<TKey> ids)
        {
            return await _dbSet
                .Where(e => ids.Contains(EF.Property<TKey>(e, "Id")))
                .ToListAsync();
        }

    }
}