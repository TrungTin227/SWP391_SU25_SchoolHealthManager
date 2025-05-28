using System.Linq.Expressions;
namespace Repositories.WorkSeeds.Interfaces
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class 
    {
        // Thêm một entity mới
        Task<TEntity> AddAsync(TEntity entity);

        // Thêm nhiều entity cùng lúc
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // Lấy tất cả các entity với điều kiện lọc tùy chọn và includes
        Task<IReadOnlyList<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes);

        // Lấy entity theo ID
        Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);

        // Cập nhật một entity
        Task UpdateAsync(TEntity entity);

        // Cập nhật nhiều entity cùng lúc
        Task UpdateRangeAsync(IEnumerable<TEntity> entities);

        // Xóa một entity
        Task DeleteAsync(TEntity entity);

        // Xóa nhiều entity cùng lúc
        Task DeleteRangeAsync(IEnumerable<TEntity> entities);
    }
}
