using DTOs.MedicationLotDTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementations
{
    public class MedicationLotRepository : GenericRepository<MedicationLot, Guid>, IMedicationLotRepository
    {
        private readonly SchoolHealthManagerDbContext _dbContext;

        public MedicationLotRepository(SchoolHealthManagerDbContext context) : base(context)
        {
            _dbContext = context;
        }

        // Override method GetByIdAsync để hỗ trợ includeDeleted
        public new async Task<MedicationLot?> GetByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = _dbContext.MedicationLots.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(ml => !ml.IsDeleted);
            }

            return await query.FirstOrDefaultAsync(ml => ml.Id == id);
        }

        // Override methods from GenericRepository without auto SaveChanges
        public new async Task<MedicationLot> AddAsync(MedicationLot entity)
        {
            var now = DateTime.UtcNow;
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            entity.IsDeleted = false;

            await _dbSet.AddAsync(entity);
            return entity;
        }

        public new async Task UpdateAsync(MedicationLot entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            // Không gọi SaveChangesAsync ở đây
        }

        public new async Task SoftDeleteAsync(MedicationLot entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            // Không gọi SaveChangesAsync ở đây
        }

        public new async Task DeleteAsync(MedicationLot entity)
        {
            _dbSet.Remove(entity);
            // Không gọi SaveChangesAsync ở đây
        }

        // ===== CÁC METHOD THỐNG KÊ TỐI ƯU HÓA HIỆU SUẤT =====

        /// <summary>
        /// Đếm số lượng lô thuốc đang hoạt động - tính toán trực tiếp trên database
        /// </summary>
        public async Task<int> GetActiveLotCountAsync()
        {
            var currentDate = DateTime.UtcNow.Date;

            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(lot => !lot.IsDeleted && lot.ExpiryDate.Date > currentDate)
                .CountAsync();
        }

        /// <summary>
        /// Đếm số lượng lô thuốc đã hết hạn - tính toán trực tiếp trên database
        /// </summary>
        public async Task<int> GetExpiredLotCountAsync()
        {
            var currentDate = DateTime.UtcNow.Date;

            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(lot => !lot.IsDeleted && lot.ExpiryDate.Date <= currentDate)
                .CountAsync();
        }

        /// <summary>
        /// Đếm số lượng lô thuốc sắp hết hạn - tính toán trực tiếp trên database
        /// </summary>
        public async Task<int> GetExpiringLotCountAsync(int daysBeforeExpiry)
        {
            var currentDate = DateTime.UtcNow.Date;
            var expiryThreshold = currentDate.AddDays(daysBeforeExpiry);

            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(lot => !lot.IsDeleted &&
                             lot.ExpiryDate.Date > currentDate &&
                             lot.ExpiryDate.Date <= expiryThreshold)
                .CountAsync();
        }

        /// <summary>
        /// Đếm tổng số lô thuốc - tính toán trực tiếp trên database
        /// </summary>
        public async Task<int> GetTotalLotCountAsync()
        {
            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(lot => !lot.IsDeleted)
                .CountAsync();
        }

        // ===== CÁC METHOD HIỆN TẠI GIỮ NGUYÊN =====

        public async Task<PagedList<MedicationLot>> GetMedicationLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            Guid? medicationId = null, bool? isExpired = null)
        {
            var query = _dbContext.MedicationLots
                .AsNoTracking()
                .Include(ml => ml.Medication)
                .Where(ml => !ml.IsDeleted);

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(ml =>
                    ml.LotNumber.Contains(searchTerm) ||
                    ml.StorageLocation.Contains(searchTerm) ||
                    ml.Medication.Name.Contains(searchTerm));
            }

            // Filter by medication
            if (medicationId.HasValue)
            {
                query = query.Where(ml => ml.MedicationId == medicationId.Value);
            }

            // Filter by expiry status
            if (isExpired.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                if (isExpired.Value)
                {
                    query = query.Where(ml => ml.ExpiryDate.Date <= today);
                }
                else
                {
                    query = query.Where(ml => ml.ExpiryDate.Date > today);
                }
            }

            // Order by expiry date (earliest first)
            query = query.OrderBy(ml => ml.ExpiryDate).ThenBy(ml => ml.LotNumber);

            return await PagedList<MedicationLot>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<List<MedicationLot>> GetExpiringLotsAsync(int daysBeforeExpiry = 30)
        {
            var thresholdDate = DateTime.UtcNow.Date.AddDays(daysBeforeExpiry);

            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Include(ml => ml.Medication)
                .Where(ml => !ml.IsDeleted &&
                            ml.ExpiryDate.Date <= thresholdDate &&
                            ml.ExpiryDate.Date > DateTime.UtcNow.Date &&
                            ml.Quantity > 0)
                .OrderBy(ml => ml.ExpiryDate)
                .ToListAsync();
        }

        public async Task<List<MedicationLot>> GetLotsByMedicationIdAsync(Guid medicationId)
        {
            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Include(ml => ml.Medication)
                .Where(ml => ml.MedicationId == medicationId && !ml.IsDeleted)
                .OrderBy(ml => ml.ExpiryDate)
                .ToListAsync();
        }

        public async Task<bool> LotNumberExistsAsync(string lotNumber, Guid? excludeId = null)
        {
            var query = _dbContext.MedicationLots
                .AsNoTracking()
                .Where(ml => ml.LotNumber.ToLower() == lotNumber.ToLower() && !ml.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(ml => ml.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetAvailableQuantityAsync(Guid medicationId)
        {
            var today = DateTime.UtcNow.Date;

            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(ml => ml.MedicationId == medicationId &&
                            !ml.IsDeleted &&
                            ml.ExpiryDate.Date > today)
                .SumAsync(ml => ml.Quantity);
        }

        public async Task<List<MedicationLot>> GetExpiredLotsAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Include(ml => ml.Medication)
                .Where(ml => !ml.IsDeleted && ml.ExpiryDate.Date <= today)
                .OrderBy(ml => ml.ExpiryDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateQuantityAsync(Guid lotId, int newQuantity)
        {
            var lot = await _dbContext.MedicationLots.FindAsync(lotId);
            if (lot == null || lot.IsDeleted)
            {
                return false;
            }

            lot.Quantity = newQuantity;
            lot.UpdatedAt = DateTime.UtcNow;

            return true;
        }

        public async Task<MedicationLot?> GetLotWithMedicationAsync(Guid lotId)
        {
            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Include(ml => ml.Medication)
                .FirstOrDefaultAsync(ml => ml.Id == lotId && !ml.IsDeleted);
        }

        public async Task<PagedList<MedicationLot>> GetSoftDeletedLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbContext.MedicationLots
                .AsNoTracking()
                .Include(ml => ml.Medication)
                .Where(ml => ml.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(ml =>
                    ml.LotNumber.Contains(searchTerm) ||
                    ml.StorageLocation.Contains(searchTerm) ||
                    ml.Medication.Name.Contains(searchTerm));
            }

            query = query.OrderByDescending(ml => ml.UpdatedAt);

            return await PagedList<MedicationLot>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<MedicationLotStatisticsResponseDTO> GetAllStatisticsAsync(DateTime currentDate, DateTime expiryThreshold)
        {
            // Use a single database query to fetch all statistics at once
            var stats = await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(ml => !ml.IsDeleted)
                .GroupBy(ml => 1) // Group all records together
                .Select(g => new MedicationLotStatisticsResponseDTO
                {
                    TotalLots = g.Count(),
                    ActiveLots = g.Count(ml => ml.ExpiryDate.Date > currentDate),
                    ExpiredLots = g.Count(ml => ml.ExpiryDate.Date <= currentDate),
                    ExpiringInNext30Days = g.Count(ml => ml.ExpiryDate.Date > currentDate &&
                                         ml.ExpiryDate.Date <= expiryThreshold),
                    GeneratedAt = DateTime.UtcNow
                })
                .FirstOrDefaultAsync() ?? new MedicationLotStatisticsResponseDTO
                {
                    GeneratedAt = DateTime.UtcNow
                };

            return stats;
        }
    }
}