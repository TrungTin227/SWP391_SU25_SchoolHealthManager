using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.WorkSeeds.Implements;

namespace Repositories.Implementations
{
    public class MedicalSupplyRepository : GenericRepository<MedicalSupply, Guid>, IMedicalSupplyRepository
    {
        private readonly ILogger<MedicalSupplyRepository> _logger;
        private readonly ICurrentTime _currentTime;

        public MedicalSupplyRepository(
            SchoolHealthManagerDbContext context,
            ILogger<MedicalSupplyRepository> logger,
            ICurrentTime currentTime) : base(context)
        {
            _logger = logger;
            _currentTime = currentTime;
        }

        #region Query Operations

        public async Task<PagedList<MedicalSupply>> GetMedicalSuppliesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            try
            {
                var query = _context.MedicalSupplies
                    .Where(ms => !ms.IsDeleted)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.Trim().ToLower();
                    query = query.Where(ms =>
                        ms.Name.ToLower().Contains(searchTermLower) ||
                        ms.Unit.ToLower().Contains(searchTermLower));
                }

                // Apply active filter
                if (isActive.HasValue)
                {
                    query = query.Where(ms => ms.IsActive == isActive.Value);
                }

                // Apply ordering - prioritize active supplies and alphabetical order
                query = query.OrderByDescending(ms => ms.IsActive)
                            .ThenBy(ms => ms.Name);

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedList<MedicalSupply>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supplies with pagination. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, IsActive: {IsActive}",
                    pageNumber, pageSize, searchTerm, isActive);
                throw;
            }
        }

        public async Task<MedicalSupply?> GetSupplyWithLotsAsync(Guid id)
        {
            try
            {
                return await _context.MedicalSupplies
                    .Include(ms => ms.Lots.Where(lot => !lot.IsDeleted))
                    .FirstOrDefaultAsync(ms => ms.Id == id && !ms.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply with lots for ID: {SupplyId}", id);
                throw;
            }
        }

        public async Task<List<MedicalSupply>> GetMedicalSuppliesByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return new List<MedicalSupply>();
                }

                var query = _context.MedicalSupplies.Where(ms => ids.Contains(ms.Id));

                if (!includeDeleted)
                {
                    query = query.Where(ms => !ms.IsDeleted);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supplies by IDs. Count: {Count}, IncludeDeleted: {IncludeDeleted}",
                    ids?.Count ?? 0, includeDeleted);
                throw;
            }
        }

        public async Task<PagedList<MedicalSupply>> GetSoftDeletedSuppliesAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var query = _context.MedicalSupplies
                    .Where(ms => ms.IsDeleted)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.Trim().ToLower();
                    query = query.Where(ms =>
                        ms.Name.ToLower().Contains(searchTermLower) ||
                        ms.Unit.ToLower().Contains(searchTermLower));
                }

                // Order by deletion date (most recent first), then by name
                query = query.OrderByDescending(ms => ms.DeletedAt)
                            .ThenBy(ms => ms.Name);

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedList<MedicalSupply>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted medical supplies. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
                    pageNumber, pageSize, searchTerm);
                throw;
            }
        }

        #endregion

        #region Business Logic Operations

        public async Task<List<MedicalSupply>> GetLowStockSuppliesAsync()
        {
            try
            {
                return await _context.MedicalSupplies
                    .Where(ms => !ms.IsDeleted &&
                                ms.IsActive &&
                                ms.MinimumStock > 0 &&
                                ms.CurrentStock <= ms.MinimumStock)
                    .OrderBy(ms => ms.CurrentStock)
                    .ThenBy(ms => ms.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock supplies");
                throw;
            }
        }

        public async Task<bool> UpdateCurrentStockAsync(Guid id, int newStock)
        {
            try
            {
                var supply = await _context.MedicalSupplies
                    .FirstOrDefaultAsync(ms => ms.Id == id && !ms.IsDeleted);

                if (supply == null)
                {
                    _logger.LogWarning("Medical supply not found for current stock update. ID: {SupplyId}", id);
                    return false;
                }

                var oldStock = supply.CurrentStock;
                supply.CurrentStock = newStock;
                supply.UpdatedAt = DateTime.UtcNow;

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Current stock updated for supply {SupplyId}. Old: {OldStock}, New: {NewStock}",
                        id, oldStock, newStock);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current stock for supply: {SupplyId}, NewStock: {NewStock}", id, newStock);
                throw;
            }
        }

        public async Task<bool> UpdateMinimumStockAsync(Guid id, int newMinimumStock)
        {
            try
            {
                var supply = await _context.MedicalSupplies
                    .FirstOrDefaultAsync(ms => ms.Id == id && !ms.IsDeleted);

                if (supply == null)
                {
                    _logger.LogWarning("Medical supply not found for minimum stock update. ID: {SupplyId}", id);
                    return false;
                }

                var oldMinimumStock = supply.MinimumStock;
                supply.MinimumStock = newMinimumStock;
                supply.UpdatedAt = DateTime.UtcNow;

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Minimum stock updated for supply {SupplyId}. Old: {OldMinimumStock}, New: {NewMinimumStock}",
                        id, oldMinimumStock, newMinimumStock);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating minimum stock for supply: {SupplyId}, NewMinimumStock: {NewMinimumStock}",
                    id, newMinimumStock);
                throw;
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return false;
                }

                var normalizedName = name.Trim().ToLower();
                var query = _context.MedicalSupplies
                    .Where(ms => !ms.IsDeleted && ms.Name.ToLower() == normalizedName);

                if (excludeId.HasValue)
                {
                    query = query.Where(ms => ms.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if medical supply name exists: {Name}, ExcludeId: {ExcludeId}",
                    name, excludeId);
                throw;
            }
        }

        #endregion

        #region Unified Batch Operations

        /// <summary>
        /// Soft delete một hoặc nhiều medical supplies
        /// Hỗ trợ cả xóa đơn lẻ (1 item) và xóa hàng loạt (nhiều items)
        /// </summary>
        public async Task<int> SoftDeleteSuppliesAsync(List<Guid> ids, Guid deletedBy)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    _logger.LogWarning("Empty or null IDs list provided for soft delete");
                    return 0;
                }

                _logger.LogInformation("Starting soft delete for {Count} medical supplies by user {DeletedBy}",
                    ids.Count, deletedBy);

                // Lấy các supplies tồn tại và chưa bị xóa
                var currentTime = _currentTime.GetVietnamTime();
                var supplies = await _context.MedicalSupplies
                    .Where(ms => ids.Contains(ms.Id) && !ms.IsDeleted)
                    .ToListAsync();

                if (!supplies.Any())
                {
                    _logger.LogWarning("No valid medical supplies found for soft delete");
                    return 0;
                }

                // Áp dụng soft delete cho tất cả supplies
                foreach (var supply in supplies)
                {
                    supply.IsDeleted = true;
                    supply.DeletedAt = currentTime;
                    supply.DeletedBy = deletedBy;
                    supply.UpdatedAt = currentTime;
                    supply.UpdatedBy = deletedBy;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully soft deleted {Count} medical supplies", supplies.Count);
                return supplies.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch soft deleting medical supplies. IDs count: {Count}, DeletedBy: {DeletedBy}",
                    ids?.Count ?? 0, deletedBy);
                throw;
            }
        }

        /// <summary>
        /// Khôi phục một hoặc nhiều medical supplies đã bị soft delete
        /// Hỗ trợ cả khôi phục đơn lẻ (1 item) và khôi phục hàng loạt (nhiều items)
        /// </summary>
        public async Task<int> RestoreSuppliesAsync(List<Guid> ids, Guid restoredBy)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    _logger.LogWarning("Empty or null IDs list provided for restore");
                    return 0;
                }

                _logger.LogInformation("Starting restore for {Count} medical supplies by user {RestoredBy}",
                    ids.Count, restoredBy);

                // Lấy các supplies đã bị soft delete
                var currentTime = _currentTime.GetVietnamTime();
                var supplies = await _context.MedicalSupplies
                    .Where(ms => ids.Contains(ms.Id) && ms.IsDeleted)
                    .ToListAsync();

                if (!supplies.Any())
                {
                    _logger.LogWarning("No soft deleted medical supplies found for restore");
                    return 0;
                }

                // Khôi phục tất cả supplies
                foreach (var supply in supplies)
                {
                    supply.IsDeleted = false;
                    supply.DeletedAt = null;
                    supply.DeletedBy = null;
                    supply.UpdatedAt = currentTime;
                    supply.UpdatedBy = restoredBy;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully restored {Count} medical supplies", supplies.Count);
                return supplies.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch restoring medical supplies. IDs count: {Count}, RestoredBy: {RestoredBy}",
                    ids?.Count ?? 0, restoredBy);
                throw;
            }
        }

        /// <summary>
        /// Xóa vĩnh viễn một hoặc nhiều medical supplies
        /// Hỗ trợ cả xóa đơn lẻ (1 item) và xóa hàng loạt (nhiều items)
        /// </summary>
        public async Task<int> PermanentDeleteSuppliesAsync(List<Guid> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    _logger.LogWarning("Empty or null IDs list provided for permanent delete");
                    return 0;
                }

                _logger.LogInformation("Starting permanent delete for {Count} medical supplies", ids.Count);

                // Lấy tất cả supplies (bao gồm cả đã bị soft delete)
                var supplies = await _context.MedicalSupplies
                    .Where(ms => ids.Contains(ms.Id))
                    .ToListAsync();

                if (!supplies.Any())
                {
                    _logger.LogWarning("No medical supplies found for permanent delete");
                    return 0;
                }

                // Xóa vĩnh viễn
                _context.MedicalSupplies.RemoveRange(supplies);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully permanently deleted {Count} medical supplies", supplies.Count);
                return supplies.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting medical supplies. IDs count: {Count}",
                    ids?.Count ?? 0);
                throw;
            }
        }

        #endregion
    }
}