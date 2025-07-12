using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementations
{
    public class MedicalSupplyRepository : GenericRepository<MedicalSupply, Guid>, IMedicalSupplyRepository
    {
        private readonly ICurrentTime _currentTime;

        public MedicalSupplyRepository(
            SchoolHealthManagerDbContext context,
            ICurrentTime currentTime) : base(context)
        {
            _currentTime = currentTime;
        }

        #region Query Operations

        public async Task<PagedList<MedicalSupply>> GetMedicalSuppliesAsync(
                int pageNumber,
                int pageSize,
                string? searchTerm = null,
                bool? isActive = null,
                bool includeDeleted = false)
        {
            // Luôn bỏ qua global filter nếu có
            var query = _context.MedicalSupplies
                .IgnoreQueryFilters() // nếu bạn dùng Global Query Filter cho IsDeleted
                .AsQueryable();

            // Lọc theo trạng thái đã xóa hay chưa
            query = query.Where(ms => ms.IsDeleted == includeDeleted);

            // Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(ms =>
                    ms.Name.ToLower().Contains(term) ||
                    ms.Unit.ToLower().Contains(term));
            }

            // Lọc theo trạng thái hoạt động
            if (isActive.HasValue)
                query = query.Where(ms => ms.IsActive == isActive.Value);

            // Sắp xếp
            query = query.OrderByDescending(ms => ms.IsActive)
                         .ThenBy(ms => ms.Name);

            // Phân trang
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<MedicalSupply>(items, totalCount, pageNumber, pageSize);
        }


        public async Task<MedicalSupply?> GetByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = _context.MedicalSupplies.AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();
            else
                query = query.Where(ms => !ms.IsDeleted);

            return await query.FirstOrDefaultAsync(ms => ms.Id == id);
        }

        public async Task<MedicalSupply?> GetSupplyWithLotsAsync(Guid id)
        {
            return await _context.MedicalSupplies
                .Where(ms => !ms.IsDeleted && ms.Id == id)
                .Include(ms => ms.Lots.Where(lot => !lot.IsDeleted))
                .FirstOrDefaultAsync();
        }

        public async Task<List<MedicalSupply>> GetMedicalSuppliesByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            if (ids == null || !ids.Any())
                return new List<MedicalSupply>();

            var query = _context.MedicalSupplies.Where(ms => ids.Contains(ms.Id));
            if (!includeDeleted)
                query = query.Where(ms => !ms.IsDeleted);

            return await query.ToListAsync();
        }

        public async Task<PagedList<MedicalSupply>> GetSoftDeletedSuppliesAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null)
        {
            var query = _context.MedicalSupplies
                .IgnoreQueryFilters()
                .Where(ms => ms.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(ms =>
                    ms.Name.ToLower().Contains(term) ||
                    ms.Unit.ToLower().Contains(term));
            }

            query = query.OrderByDescending(ms => ms.DeletedAt)
                         .ThenBy(ms => ms.Name);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<MedicalSupply>(items, totalCount, pageNumber, pageSize);
        }

        #endregion

        #region Business Logic Operations

        public async Task<List<MedicalSupply>> GetLowStockSuppliesAsync()
        {
            return await _context.MedicalSupplies
                .Where(ms => !ms.IsDeleted && ms.IsActive && ms.MinimumStock > 0 && ms.CurrentStock <= ms.MinimumStock)
                .OrderBy(ms => ms.CurrentStock)
                .ThenBy(ms => ms.Name)
                .ToListAsync();
        }

        public async Task<bool> UpdateCurrentStockAsync(Guid id, int newStock)
        {
            var supply = await _context.MedicalSupplies
                .Where(ms => !ms.IsDeleted && ms.Id == id)
                .FirstOrDefaultAsync();
            if (supply == null) return false;

            supply.CurrentStock = newStock;
            supply.UpdatedAt = _currentTime.GetVietnamTime();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMinimumStockAsync(Guid id, int newMinimumStock)
        {
            var supply = await _context.MedicalSupplies
                .Where(ms => !ms.IsDeleted && ms.Id == id)
                .FirstOrDefaultAsync();
            if (supply == null) return false;

            supply.MinimumStock = newMinimumStock;
            supply.UpdatedAt = _currentTime.GetVietnamTime();
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Validation Operations

        public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            var query = _context.MedicalSupplies.AsQueryable();
            query = query.Where(ms => !ms.IsDeleted && ms.Name.ToLower() == normalized);
            if (excludeId.HasValue)
                query = query.Where(ms => ms.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        #endregion

        #region Unified Batch Operations

        public async Task<int> SoftDeleteSuppliesAsync(List<Guid> ids, Guid deletedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();
            var supplies = await _context.MedicalSupplies.Where(ms => ids.Contains(ms.Id) && !ms.IsDeleted).ToListAsync();
            supplies.ForEach(ms => {
                ms.IsDeleted = true; ms.DeletedAt = currentTime; ms.DeletedBy = deletedBy;
                ms.UpdatedAt = currentTime; ms.UpdatedBy = deletedBy;
            });
            await _context.SaveChangesAsync();
            return supplies.Count;
        }

        public async Task<int> RestoreSuppliesAsync(List<Guid> ids, Guid restoredBy)
        {
            var currentTime = _currentTime.GetVietnamTime();
            var supplies = await _context.MedicalSupplies.IgnoreQueryFilters()
                .Where(ms => ids.Contains(ms.Id) && ms.IsDeleted).ToListAsync();
            supplies.ForEach(ms => {
                ms.IsDeleted = false; ms.DeletedAt = null; ms.DeletedBy = null;
                ms.UpdatedAt = currentTime; ms.UpdatedBy = restoredBy;
            });
            await _context.SaveChangesAsync();
            return supplies.Count;
        }

        public async Task<int> PermanentDeleteSuppliesAsync(List<Guid> ids)
        {
            var supplies = await _context.MedicalSupplies.IgnoreQueryFilters()
                .Where(ms => ids.Contains(ms.Id)).ToListAsync();
            _context.MedicalSupplies.RemoveRange(supplies);
            await _context.SaveChangesAsync();
            return supplies.Count;
        }

        #endregion
    }
}
