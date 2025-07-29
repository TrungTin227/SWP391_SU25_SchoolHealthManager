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
            // Start with base query
            var query = _context.MedicalSupplies
                .IgnoreQueryFilters()
                .AsQueryable();

            // Filter by deleted status
            query = query.Where(ms => ms.IsDeleted == includeDeleted);

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(ms =>
                    ms.Name.ToLower().Contains(term) ||
                    ms.Unit.ToLower().Contains(term));
            }

            // Filter by active status
            if (isActive.HasValue)
                query = query.Where(ms => ms.IsActive == isActive.Value);

            // Sort
            query = query.OrderByDescending(ms => ms.IsActive)
                         .ThenBy(ms => ms.Name);

            // Include lots (only non-deleted)
            var finalQuery = query
                .Include(ms => ms.Lots.Where(l => !l.IsDeleted));

            // Pagination
            var totalCount = await finalQuery.CountAsync();
            var items = await finalQuery
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
            // 1. Lấy tất cả các vật tư đang hoạt động và có đặt mức tối thiểu
            //    Bắt buộc phải .Include(ms => ms.Lots) để có thể truy cập thuộc tính CurrentStock
            var allActiveSupplies = await _context.MedicalSupplies
                .Include(ms => ms.Lots.Where(l => !l.IsDeleted))
                .Where(ms => !ms.IsDeleted && ms.IsActive && ms.MinimumStock > 0)
                .ToListAsync(); // <-- Chuyển sang thực thi trong bộ nhớ

            // 2. Lọc trong bộ nhớ bằng cách sử dụng thuộc tính CurrentStock (đã được tính toán)
            var lowStockSupplies = allActiveSupplies
                .Where(ms => ms.CurrentStock <= ms.MinimumStock) // Logic này giờ chạy trên C#
                .OrderBy(ms => ms.CurrentStock)
                .ThenBy(ms => ms.Name)
                .ToList();

            return lowStockSupplies;
        }

        public async Task<bool> ReconcileStockAsync(Guid supplyId, int actualPhysicalCount)
        {
            // Phải Include("Lots") để có thể tính toán CurrentStock chính xác
            var supply = await _context.MedicalSupplies
                .Include(s => s.Lots)
                .FirstOrDefaultAsync(ms => ms.Id == supplyId && !ms.IsDeleted);

            if (supply == null) return false;

            // Lấy tồn kho hiện tại trên hệ thống (được tính toán tự động)
            int systemStock = supply.CurrentStock;

            // Tính toán lượng chênh lệch cần điều chỉnh
            int adjustmentQuantity = actualPhysicalCount - systemStock;

            if (adjustmentQuantity == 0)
            {
                // Không có gì để thay đổi
                return true;
            }

            // Tạo một lô hàng đặc biệt để ghi nhận sự điều chỉnh này
            var adjustmentLot = new MedicalSupplyLot
            {
                Id = Guid.NewGuid(),
                MedicalSupplyId = supply.Id,
                // Dùng số lô đặc biệt để dễ nhận biết
                LotNumber = $"ADJUST-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Quantity = adjustmentQuantity, // Có thể là số âm (nếu thất thoát) hoặc dương (nếu dư)
                                               // Lô điều chỉnh thường không có ngày hết hạn thực tế, hoặc có thể gán một ngày rất xa
                ExpirationDate = DateTime.UtcNow.AddYears(10),
                CreatedAt = _currentTime.GetVietnamTime(),
            };

            await _context.MedicalSupplyLots.AddAsync(adjustmentLot);
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
