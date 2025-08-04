using Microsoft.EntityFrameworkCore;
using Repositories.WorkSeeds.Implements;
using System.Linq.Expressions;

namespace Repositories.Implementations
{
    public class MedicationLotRepository : GenericRepository<MedicationLot, Guid>, IMedicationLotRepository
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ICurrentTime _currentTime;

        public MedicationLotRepository(SchoolHealthManagerDbContext context, ICurrentTime currentTime) : base(context)
        {
            _dbContext = context;
            _currentTime = currentTime;
        }

        #region Basic CRUD Methods

        public async Task<PagedList<MedicationLot>> GetMedicationLotsAsync(
    int pageNumber, int pageSize, string? searchTerm = null,
    Guid? medicationId = null, bool? isExpired = null,
    int? daysBeforeExpiry = null, bool includeDeleted = false)
        {
            // 1. Lấy các lô thuốc THẬT từ cơ sở dữ liệu
            var realLotsQuery = _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Include(ml => ml.Medication)
                .Where(ml => ml.MedicationId != null)
                .Where(ml => ml.IsDeleted == includeDeleted);

            // Lọc theo thuốc (giữ nguyên)
            if (medicationId != null)
                realLotsQuery = realLotsQuery.Where(ml => ml.MedicationId == medicationId);

            // Lọc theo từ khóa (giữ nguyên)
            if (!string.IsNullOrWhiteSpace(searchTerm))
                realLotsQuery = realLotsQuery.Where(ml =>
                    ml.LotNumber.Contains(searchTerm) ||
                    ml.Medication.Name.Contains(searchTerm));

            // Lọc theo tình trạng hết hạn (giữ nguyên)
            if (isExpired.HasValue)
            {
                if (isExpired.Value)
                    realLotsQuery = realLotsQuery.Where(ml => ml.ExpiryDate.Date <= DateTime.UtcNow.Date);
                else
                    realLotsQuery = realLotsQuery.Where(ml => ml.ExpiryDate.Date > DateTime.UtcNow.Date);
            }

            if (daysBeforeExpiry.HasValue)
            {
                var limitDate = DateTime.UtcNow.AddDays(daysBeforeExpiry.Value);
                realLotsQuery = realLotsQuery.Where(ml => ml.ExpiryDate.Date <= limitDate.Date);
            }

            var realLots = await realLotsQuery
                .OrderBy(ml => ml.ExpiryDate)
                .ThenBy(ml => ml.LotNumber)
                .ToListAsync();

            // 2. Thuốc không có lô (logic này vẫn đúng và không bị ảnh hưởng)
            var medsWithoutLot = await _dbContext.Medications
                .Where(m => !m.IsDeleted)
                .Where(m => medicationId == null || m.Id == medicationId)
                .Where(m => string.IsNullOrWhiteSpace(searchTerm) || m.Name.Contains(searchTerm))
                // Dòng này hoạt động đúng vì realLotsQuery giờ chỉ chứa các MedicationId khác null
                .Where(m => !realLotsQuery.Select(l => l.MedicationId).Contains(m.Id))
                .ToListAsync();

            // 3. Tạo lô ảo (giữ nguyên)
            var virtualLots = medsWithoutLot.Select(m => new MedicationLot
            {
                Id = Guid.Empty,
                MedicationId = m.Id,
                Medication = m,
                LotNumber = "Tồn kho chung",
                Quantity = 0,
                ExpiryDate = DateTime.MaxValue,
                StorageLocation = "-",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // 4. Gộp và phân trang (giữ nguyên)
            var combined = realLots
                .Concat(virtualLots)
                .OrderBy(ml => ml.Medication?.Name ?? "")
                .ThenBy(ml => ml.LotNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = await realLotsQuery.CountAsync() + medsWithoutLot.Count;

            return new PagedList<MedicationLot>(combined, totalCount, pageNumber, pageSize);
        }
        public async Task<MedicationLot?> GetLotWithMedicationAsync(Guid lotId)
        {
            return await FirstOrDefaultAsync(
                ml => ml.Id == lotId && !ml.IsDeleted,
                ml => ml.Medication
            );
        }

        public new async Task<MedicationLot?> GetByIdAsync(Guid id, bool includeDeleted = false)
        {
            if (includeDeleted)
            {
                return await _dbContext.MedicationLots
                    .IgnoreQueryFilters()
                    .Include(ml => ml.Medication)
                    .FirstOrDefaultAsync(ml => ml.Id == id);
            }

            return await _dbContext.MedicationLots
                .Include(ml => ml.Medication)
                .FirstOrDefaultAsync(ml => ml.Id == id && !ml.IsDeleted);
        }

        #endregion

        #region Batch Operations (Unified - Support Single and Multiple)

        /// <summary>
        /// Lấy danh sách lô thuốc theo IDs - hỗ trợ cả single và multiple
        /// </summary>
        public async Task<List<MedicationLot>> GetMedicationLotsByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            if (ids == null || !ids.Any())
                return new List<MedicationLot>();

            var query = _dbContext.MedicationLots
                .Include(ml => ml.Medication)
                .Where(ml => ids.Contains(ml.Id));

            if (!includeDeleted)
            {
                query = query.Where(ml => !ml.IsDeleted);
            }
            else
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Soft delete một hoặc nhiều lô thuốc - hỗ trợ cả single và multiple
        /// </summary>
        public async Task<int> SoftDeleteLotsAsync(List<Guid> ids, Guid deletedBy)
        {
            if (ids == null || !ids.Any())
                return 0;

            var lots = await _dbContext.MedicationLots
                .Where(ml => ids.Contains(ml.Id) && !ml.IsDeleted)
                .ToListAsync();

            if (!lots.Any())
                return 0;

            var now = _currentTime.GetVietnamTime();
            foreach (var lot in lots)
            {
                lot.IsDeleted = true;
                lot.DeletedAt = now;
                lot.DeletedBy = deletedBy;
                lot.UpdatedAt = now;
                lot.UpdatedBy = deletedBy;
            }

            _dbContext.MedicationLots.UpdateRange(lots);
            return lots.Count;
        }

        /// <summary>
        /// Khôi phục một hoặc nhiều lô thuốc - hỗ trợ cả single và multiple
        /// </summary>
        public async Task<int> RestoreLotsAsync(List<Guid> ids, Guid restoredBy)
        {
            if (ids == null || !ids.Any())
                return 0;

            var lots = await _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Where(ml => ids.Contains(ml.Id) && ml.IsDeleted)
                .ToListAsync();

            if (!lots.Any())
                return 0;

            var now = _currentTime.GetVietnamTime();
            foreach (var lot in lots)
            {
                lot.IsDeleted = false;
                lot.DeletedAt = null;
                lot.DeletedBy = null;
                lot.UpdatedAt = now;
                lot.UpdatedBy = restoredBy;
            }

            _dbContext.MedicationLots.UpdateRange(lots);
            return lots.Count;
        }

        /// <summary>
        /// Xóa vĩnh viễn một hoặc nhiều lô thuốc - hỗ trợ cả single và multiple
        /// </summary>
        public async Task<int> PermanentDeleteLotsAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return 0;

            var lots = await _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Where(ml => ids.Contains(ml.Id))
                .ToListAsync();

            if (!lots.Any())
                return 0;

            _dbContext.MedicationLots.RemoveRange(lots);
            return lots.Count;
        }

        #endregion

        #region Soft Delete Operations

        public async Task<PagedList<MedicationLot>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var predicate = BuildSoftDeletedPredicate(searchTerm);

            var query = _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Include(ml => ml.Medication)
                .Where(predicate)
                .OrderByDescending(ml => ml.UpdatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<MedicationLot>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<int> PermanentDeleteExpiredLotsAsync(int daysToExpire = 30)
        {
            var expiredDate = _currentTime.GetVietnamTime().AddDays(-daysToExpire);
            var predicate = BuildPermanentDeletePredicate(expiredDate);

            var expiredLots = await _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Where(predicate)
                .ToListAsync();

            if (!expiredLots.Any())
                return 0;

            var count = expiredLots.Count;
            _dbContext.MedicationLots.RemoveRange(expiredLots);

            return count;
        }

        #endregion

        #region Business Logic Methods

        public async Task<List<MedicationLot>> GetExpiringLotsAsync(int daysBeforeExpiry = 30)
        {
            var predicate = BuildExpiringLotsPredicate(daysBeforeExpiry);

            var lots = await GetAllAsync(
                predicate: predicate,
                orderBy: q => q.OrderBy(ml => ml.ExpiryDate),
                includes: ml => ml.Medication
            );

            return lots.ToList();
        }

        public async Task<List<MedicationLot>> GetExpiredLotsAsync()
        {
            var predicate = BuildExpiredLotsPredicate();

            var lots = await GetAllAsync(
                predicate: predicate,
                orderBy: q => q.OrderBy(ml => ml.ExpiryDate),
                includes: ml => ml.Medication
            );

            return lots.ToList();
        }

        public async Task<List<MedicationLot>> GetLotsByMedicationIdAsync(Guid medicationId)
        {
            var predicate = BuildLotsByMedicationIdPredicate(medicationId);

            var lots = await GetAllAsync(
                predicate: predicate,
                orderBy: q => q.OrderBy(ml => ml.ExpiryDate),
                includes: ml => ml.Medication
            );

            return lots.ToList();
        }

        public async Task<int> GetAvailableQuantityAsync(Guid medicationId)
        {
            var today = _currentTime.GetVietnamTime().Date;

            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(ml => ml.MedicationId == medicationId &&
                            !ml.IsDeleted &&
                            ml.ExpiryDate.Date > today)
                .SumAsync(ml => ml.Quantity);
        }

        public async Task<bool> UpdateQuantityAsync(Guid lotId, int newQuantity)
        {
            var lot = await GetByIdAsync(lotId);
            if (lot == null || lot.IsDeleted)
            {
                return false;
            }

            lot.Quantity = newQuantity;
            lot.UpdatedAt = _currentTime.GetVietnamTime();

            await UpdateAsync(lot);
            return true;
        }

        public async Task<bool> LotNumberExistsAsync(string lotNumber, Guid? excludeId = null)
        {
            var predicate = BuildLotNumberExistsPredicate(lotNumber, excludeId);
            return await AnyAsync(predicate);
        }

        #endregion

        #region Statistics Methods

        public async Task<MedicationLotStatisticsResponseDTO> GetAllStatisticsAsync(DateTime currentDate, DateTime expiryThreshold)
        {
            var stats = await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(ml => !ml.IsDeleted)
                .GroupBy(ml => 1)
                .Select(g => new MedicationLotStatisticsResponseDTO
                {
                    TotalLots = g.Count(),
                    ActiveLots = g.Count(ml => ml.ExpiryDate.Date > currentDate),
                    ExpiredLots = g.Count(ml => ml.ExpiryDate.Date <= currentDate),
                    ExpiringInNext30Days = g.Count(ml => ml.ExpiryDate.Date > currentDate &&
                                         ml.ExpiryDate.Date <= expiryThreshold),
                    GeneratedAt = _currentTime.GetVietnamTime()
                })
                .FirstOrDefaultAsync();

            // Nếu không có data, tạo object mới với giá trị mặc định
            if (stats == null)
            {
                return new MedicationLotStatisticsResponseDTO
                {
                    TotalLots = 0,
                    ActiveLots = 0,
                    ExpiredLots = 0,
                    ExpiringInNext30Days = 0,
                    GeneratedAt = _currentTime.GetVietnamTime()
                };
            }

            // Percentages sẽ được tính tự động trong DTO
            return stats;
        }

        #endregion

        #region Private Helper Methods - Predicate Builders

        private Expression<Func<MedicationLot, bool>> BuildMedicationLotPredicate(
    string? searchTerm, Guid? medicationId, bool? isExpired,
    int? daysBeforeExpiry, bool includeDeleted)
        {
            var today = _currentTime.GetVietnamTime().Date;

            return ml =>
                // Chính xác hóa Deleted filter
                ml.IsDeleted == includeDeleted &&

                // Search term filter
                (string.IsNullOrWhiteSpace(searchTerm) ||
                 ml.LotNumber.Contains(searchTerm) ||
                 ml.StorageLocation.Contains(searchTerm) ||
                 ml.Medication.Name.Contains(searchTerm)) &&

                // Medication ID filter
                (!medicationId.HasValue || ml.MedicationId == medicationId.Value) &&

                // Expired filter
                (!isExpired.HasValue ||
                 (isExpired.Value && ml.ExpiryDate.Date <= today) ||
                 (!isExpired.Value && ml.ExpiryDate.Date > today)) &&

                // Days before expiry filter
                (!daysBeforeExpiry.HasValue ||
                 (ml.ExpiryDate.Date <= today.AddDays(daysBeforeExpiry.Value) &&
                  ml.ExpiryDate.Date > today));
        }


        private Expression<Func<MedicationLot, bool>> BuildExpiringLotsPredicate(int daysBeforeExpiry)
        {
            var today = _currentTime.GetVietnamTime().Date;
            var thresholdDate = today.AddDays(daysBeforeExpiry);

            return ml => !ml.IsDeleted &&
                        ml.ExpiryDate.Date <= thresholdDate &&
                        ml.ExpiryDate.Date > today &&
                        ml.Quantity > 0;
        }

        private Expression<Func<MedicationLot, bool>> BuildLotsByMedicationIdPredicate(Guid medicationId)
        {
            return ml => ml.MedicationId == medicationId && !ml.IsDeleted;
        }

        private Expression<Func<MedicationLot, bool>> BuildLotNumberExistsPredicate(string lotNumber, Guid? excludeId)
        {
            return ml => ml.LotNumber.ToLower() == lotNumber.ToLower() &&
                        !ml.IsDeleted &&
                        (!excludeId.HasValue || ml.Id != excludeId.Value);
        }

        private Expression<Func<MedicationLot, bool>> BuildExpiredLotsPredicate()
        {
            var today = _currentTime.GetVietnamTime().Date;
            return ml => !ml.IsDeleted && ml.ExpiryDate.Date <= today;
        }

        private Expression<Func<MedicationLot, bool>> BuildSoftDeletedPredicate(string? searchTerm)
        {
            return ml => ml.IsDeleted &&
                        (string.IsNullOrWhiteSpace(searchTerm) ||
                         ml.LotNumber.Contains(searchTerm) ||
                         ml.StorageLocation.Contains(searchTerm) ||
                         ml.Medication.Name.Contains(searchTerm));
        }

        private Expression<Func<MedicationLot, bool>> BuildPermanentDeletePredicate(DateTime expiredDate)
        {
            return ml => ml.IsDeleted &&
                        ml.DeletedAt.HasValue &&
                        ml.DeletedAt <= expiredDate;
        }

        #endregion
    }
}