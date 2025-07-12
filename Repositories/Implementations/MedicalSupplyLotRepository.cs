using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Implementations
{
    public class MedicalSupplyLotRepository : GenericRepository<MedicalSupplyLot, Guid>, IMedicalSupplyLotRepository
    {
        private readonly ILogger<MedicalSupplyLotRepository> _logger;
        private readonly ICurrentTime _currentTime;

        public MedicalSupplyLotRepository(
            SchoolHealthManagerDbContext context,
            ILogger<MedicalSupplyLotRepository> logger,
            ICurrentTime currentTime) : base(context)
        {
            _logger = logger;
            _currentTime = currentTime;
        }

        #region Query Operations

        public async Task<PagedList<MedicalSupplyLot>> GetMedicalSupplyLotsAsync(
             int pageNumber, int pageSize, string? searchTerm = null,
             Guid? medicalSupplyId = null, bool? isExpired = null, bool includeDeleted = false)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;

                var query = _context.MedicalSupplyLots
                    .IgnoreQueryFilters() // nếu bạn có GlobalFilter cho IsDeleted
                    .Include(msl => msl.MedicalSupply)
                    .Where(msl => msl.IsDeleted == includeDeleted) //  lọc theo đúng yêu cầu
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(msl =>
                        msl.LotNumber.ToLower().Contains(searchTermLower) ||
                        msl.MedicalSupply.Name.ToLower().Contains(searchTermLower));
                }

                // Medical supply filter
                if (medicalSupplyId.HasValue)
                {
                    query = query.Where(msl => msl.MedicalSupplyId == medicalSupplyId.Value);
                }

                // Expiry filter
                if (isExpired.HasValue)
                {
                    if (isExpired.Value)
                        query = query.Where(msl => msl.ExpirationDate.Date <= currentDate);
                    else
                        query = query.Where(msl => msl.ExpirationDate.Date > currentDate);
                }

                // Ordering
                query = query.OrderBy(msl => msl.ExpirationDate)
                             .ThenBy(msl => msl.LotNumber);

                // Pagination
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedList<MedicalSupplyLot>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply lots with pagination");
                throw;
            }
        }

        public async Task<MedicalSupplyLot?> GetLotWithSupplyAsync(Guid id)
        {
            try
            {
                return await _context.MedicalSupplyLots
                    .Include(msl => msl.MedicalSupply)
                    .FirstOrDefaultAsync(msl => msl.Id == id && !msl.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply lot with supply for ID: {LotId}", id);
                throw;
            }
        }

        public async Task<List<MedicalSupplyLot>> GetMedicalSupplyLotsByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return new List<MedicalSupplyLot>();

                var query = _context.MedicalSupplyLots
                    .Include(msl => msl.MedicalSupply)
                    .Where(msl => ids.Contains(msl.Id));

                if (!includeDeleted)
                {
                    query = query.Where(msl => !msl.IsDeleted);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical supply lots by IDs");
                throw;
            }
        }

        public async Task<PagedList<MedicalSupplyLot>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var query = _context.MedicalSupplyLots
                    .Include(msl => msl.MedicalSupply)
                    .Where(msl => msl.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(msl =>
                        msl.LotNumber.ToLower().Contains(searchTermLower) ||
                        msl.MedicalSupply.Name.ToLower().Contains(searchTermLower));
                }

                query = query.OrderByDescending(msl => msl.DeletedAt)
                            .ThenBy(msl => msl.LotNumber);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedList<MedicalSupplyLot>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted medical supply lots");
                throw;
            }
        }

        #endregion

        #region Business Logic Operations

        public async Task<List<MedicalSupplyLot>> GetExpiringLotsAsync(int daysBeforeExpiry = 30)
        {
            try
            {
                var targetDate = DateTime.UtcNow.Date.AddDays(daysBeforeExpiry);
                var currentDate = DateTime.UtcNow.Date;

                return await _context.MedicalSupplyLots
                    .Include(msl => msl.MedicalSupply)
                    .Where(msl => !msl.IsDeleted &&
                                  msl.ExpirationDate.Date > currentDate &&
                                  msl.ExpirationDate.Date <= targetDate &&
                                  msl.Quantity > 0)
                    .OrderBy(msl => msl.ExpirationDate)
                    .ThenBy(msl => msl.LotNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring medical supply lots");
                throw;
            }
        }

        public async Task<List<MedicalSupplyLot>> GetExpiredLotsAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;

                return await _context.MedicalSupplyLots
                    .Include(msl => msl.MedicalSupply)
                    .Where(msl => !msl.IsDeleted &&
                                  msl.ExpirationDate.Date <= currentDate &&
                                  msl.Quantity > 0)
                    .OrderBy(msl => msl.ExpirationDate)
                    .ThenBy(msl => msl.LotNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired medical supply lots");
                throw;
            }
        }

        public async Task<List<MedicalSupplyLot>> GetLotsByMedicalSupplyIdAsync(Guid medicalSupplyId)
        {
            try
            {
                return await _context.MedicalSupplyLots
                    .Include(msl => msl.MedicalSupply)
                    .Where(msl => !msl.IsDeleted &&
                                  msl.MedicalSupplyId == medicalSupplyId)
                    .OrderBy(msl => msl.ExpirationDate)
                    .ThenBy(msl => msl.LotNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lots by medical supply ID: {SupplyId}", medicalSupplyId);
                throw;
            }
        }

        public async Task<int> GetAvailableQuantityAsync(Guid medicalSupplyId)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;

                return await _context.MedicalSupplyLots
                    .Where(msl => !msl.IsDeleted &&
                                  msl.MedicalSupplyId == medicalSupplyId &&
                                  msl.ExpirationDate.Date > currentDate)
                    .SumAsync(msl => (int?)msl.Quantity) ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available quantity for medical supply: {SupplyId}", medicalSupplyId);
                throw;
            }
        }

        public async Task<bool> UpdateQuantityAsync(Guid lotId, int newQuantity)
        {
            try
            {
                var lot = await _context.MedicalSupplyLots
                    .FirstOrDefaultAsync(msl => msl.Id == lotId && !msl.IsDeleted);

                if (lot == null)
                    return false;

                lot.Quantity = newQuantity;
                lot.UpdatedAt = _currentTime.GetVietnamTime();

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for lot: {LotId}", lotId);
                throw;
            }
        }

        public async Task<int> CalculateCurrentStockForSupplyAsync(Guid medicalSupplyId)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;

                return await _context.MedicalSupplyLots
                    .Where(msl => !msl.IsDeleted &&
                                  msl.MedicalSupplyId == medicalSupplyId &&
                                  msl.ExpirationDate.Date > currentDate)
                    .SumAsync(msl => (int?)msl.Quantity) ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating current stock for supply: {SupplyId}", medicalSupplyId);
                throw;
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> LotNumberExistsAsync(string lotNumber, Guid? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lotNumber))
                    return false;

                var query = _context.MedicalSupplyLots
                    .Where(msl => !msl.IsDeleted &&
                                  msl.LotNumber.ToLower() == lotNumber.ToLower());

                if (excludeId.HasValue)
                {
                    query = query.Where(msl => msl.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if lot number exists: {LotNumber}", lotNumber);
                throw;
            }
        }

        #endregion

        #region Unified Delete & Restore Operations

        public async Task<int> SoftDeleteLotsAsync(List<Guid> ids, Guid deletedBy)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return 0;

                var currentTime = _currentTime.GetVietnamTime();
                var lots = await _context.MedicalSupplyLots
                    .Where(msl => ids.Contains(msl.Id) && !msl.IsDeleted)
                    .ToListAsync();

                if (!lots.Any())
                    return 0;

                foreach (var lot in lots)
                {
                    lot.IsDeleted = true;
                    lot.DeletedAt = currentTime;
                    lot.DeletedBy = deletedBy;
                    lot.UpdatedAt = currentTime;
                    lot.UpdatedBy = deletedBy;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully soft deleted {Count} medical supply lots", lots.Count);
                return lots.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch soft deleting medical supply lots");
                throw;
            }
        }

        public async Task<int> RestoreLotsAsync(List<Guid> ids, Guid restoredBy)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return 0;

                var currentTime = _currentTime.GetVietnamTime();
                var lots = await _context.MedicalSupplyLots
                    .Where(msl => ids.Contains(msl.Id) && msl.IsDeleted)
                    .ToListAsync();

                if (!lots.Any())
                    return 0;

                foreach (var lot in lots)
                {
                    lot.IsDeleted = false;
                    lot.DeletedAt = null;
                    lot.DeletedBy = null;
                    lot.UpdatedAt = currentTime;
                    lot.UpdatedBy = restoredBy;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully restored {Count} medical supply lots", lots.Count);
                return lots.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch restoring medical supply lots");
                throw;
            }
        }

        public async Task<int> PermanentDeleteLotsAsync(List<Guid> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return 0;

                var lots = await _context.MedicalSupplyLots
                    .Where(msl => ids.Contains(msl.Id))
                    .ToListAsync();

                if (!lots.Any())
                    return 0;

                _context.MedicalSupplyLots.RemoveRange(lots);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully permanently deleted {Count} medical supply lots", lots.Count);
                return lots.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting medical supply lots");
                throw;
            }
        }

        public async Task<int> PermanentDeleteExpiredLotsAsync(int daysExpired = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Date.AddDays(-daysExpired);

                var expiredLots = await _context.MedicalSupplyLots
                    .Where(msl => msl.IsDeleted &&
                                  msl.ExpirationDate.Date <= cutoffDate)
                    .ToListAsync();

                if (!expiredLots.Any())
                    return 0;

                _context.MedicalSupplyLots.RemoveRange(expiredLots);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully permanently deleted {Count} expired lots", expiredLots.Count);
                return expiredLots.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting expired lots");
                throw;
            }
        }

        #endregion

        #region Additional Helper Methods

        public async Task<bool> HasActiveLotsAsync(Guid medicalSupplyId)
        {
            try
            {
                return await _context.MedicalSupplyLots
                    .Where(lot => lot.MedicalSupplyId == medicalSupplyId && !lot.IsDeleted)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if medical supply has active lots: {SupplyId}", medicalSupplyId);
                throw;
            }
        }

        #endregion
    }
}