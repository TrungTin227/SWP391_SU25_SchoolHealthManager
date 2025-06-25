using DTOs.VaccineLotDTOs.Response;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.Implementations
{
    public class VaccineLotRepository : GenericRepository<MedicationLot, Guid>, IVaccineLotRepository
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ICurrentTime _currentTime;

        public VaccineLotRepository(SchoolHealthManagerDbContext context, ICurrentTime currentTime) : base(context)
        {
            _dbContext = context;
            _currentTime = currentTime;
        }

        #region Basic CRUD Methods

        public async Task<PagedList<MedicationLot>> GetVaccineLotsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            Guid? vaccineTypeId = null,
            bool? isExpired = null,
            int? daysBeforeExpiry = null,
            bool? isDeleted = null)
        {
            var predicate = BuildVaccineLotPredicate(
                searchTerm, vaccineTypeId, isExpired, daysBeforeExpiry, isDeleted);

            return await GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                orderBy: q => q.OrderBy(ml => ml.ExpiryDate).ThenBy(ml => ml.LotNumber),
                includes: ml => ml.VaccineType
            );
        }

        public async Task<MedicationLot?> GetVaccineLotWithDetailsAsync(Guid lotId)
        {
            return await FirstOrDefaultAsync(
                ml => ml.Id == lotId && !ml.IsDeleted && ml.Type == LotType.Vaccine,
                ml => ml.VaccineType
            );
        }

        public async Task<MedicationLot?> GetVaccineLotByIdAsync(Guid id, bool includeDeleted = false)
        {
            if (includeDeleted)
            {
                return await _dbContext.MedicationLots
                    .IgnoreQueryFilters()
                    .Include(ml => ml.VaccineType)
                    .FirstOrDefaultAsync(ml => ml.Id == id && ml.Type == LotType.Vaccine);
            }

            return await _dbContext.MedicationLots
                .Include(ml => ml.VaccineType)
                .FirstOrDefaultAsync(ml => ml.Id == id && !ml.IsDeleted && ml.Type == LotType.Vaccine);
        }

        #endregion

        #region Batch Operations

        public async Task<int> SoftDeleteVaccineLotsAsync(List<Guid> ids, Guid deletedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            var count = await _dbContext.MedicationLots
                .Where(ml => ids.Contains(ml.Id) && !ml.IsDeleted && ml.Type == LotType.Vaccine)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ml => ml.IsDeleted, true)
                    .SetProperty(ml => ml.DeletedAt, currentTime)
                    .SetProperty(ml => ml.DeletedBy, deletedBy)
                    .SetProperty(ml => ml.UpdatedAt, currentTime)
                    .SetProperty(ml => ml.UpdatedBy, deletedBy));

            return count;
        }

        public async Task<int> RestoreVaccineLotsAsync(List<Guid> ids, Guid restoredBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            var count = await _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Where(ml => ids.Contains(ml.Id) && ml.IsDeleted && ml.Type == LotType.Vaccine)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ml => ml.IsDeleted, false)
                    .SetProperty(ml => ml.DeletedAt, (DateTime?)null)
                    .SetProperty(ml => ml.DeletedBy, (Guid?)null)
                    .SetProperty(ml => ml.UpdatedAt, currentTime)
                    .SetProperty(ml => ml.UpdatedBy, restoredBy));

            return count;
        }

        #endregion

        #region Vaccine-Specific Operations

        public async Task<List<MedicationLot>> GetLotsByVaccineTypeAsync(Guid vaccineTypeId)
        {
            var predicate = BuildLotsByVaccineTypePredicate(vaccineTypeId);

            var lots = await GetAllAsync(
                predicate: predicate,
                orderBy: q => q.OrderBy(ml => ml.ExpiryDate),
                includes: ml => ml.VaccineType
            );

            return lots.ToList();
        }

        public async Task<List<MedicationLot>> GetExpiringVaccineLotsAsync(int daysBeforeExpiry = 30)
        {
            var predicate = BuildExpiringVaccineLotsPredicate(daysBeforeExpiry);

            var lots = await GetAllAsync(
                predicate: predicate,
                orderBy: q => q.OrderBy(ml => ml.ExpiryDate),
                includes: ml => ml.VaccineType
            );

            return lots.ToList();
        }

        public async Task<List<MedicationLot>> GetExpiredVaccineLotsAsync()
        {
            var predicate = BuildExpiredVaccineLotsPredicate();

            var lots = await GetAllAsync(
                predicate: predicate,
                orderBy: q => q.OrderBy(ml => ml.ExpiryDate),
                includes: ml => ml.VaccineType
            );

            return lots.ToList();
        }

        public async Task<bool> UpdateVaccineQuantityAsync(Guid lotId, int newQuantity)
        {
            var lot = await GetVaccineLotByIdAsync(lotId);
            if (lot == null || lot.IsDeleted)
            {
                return false;
            }

            lot.Quantity = newQuantity;
            lot.UpdatedAt = _currentTime.GetVietnamTime();

            await UpdateAsync(lot);
            return true;
        }

        public async Task<bool> VaccineLotNumberExistsAsync(string lotNumber, Guid? excludeId = null)
        {
            var predicate = BuildVaccineLotNumberExistsPredicate(lotNumber, excludeId);
            return await AnyAsync(predicate);
        }

        #endregion

        #region Soft Delete Operations

        public async Task<PagedList<MedicationLot>> GetSoftDeletedVaccineLotsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var predicate = BuildSoftDeletedVaccinePredicate(searchTerm);

            // Tự viết query thay vì dùng GetPagedAsync vì cần IgnoreQueryFilters
            var query = _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Include(ml => ml.VaccineType)
                .Where(predicate)
                .OrderByDescending(ml => ml.DeletedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<MedicationLot>(items, totalCount, pageNumber, pageSize);
        }

        #endregion

        #region Statistics Methods

        public async Task<VaccineLotStatisticsResponseDTO> GetVaccineLotStatisticsAsync(DateTime currentDate, DateTime expiryThreshold)
        {
            var statistics = await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(ml => ml.Type == LotType.Vaccine)
                .GroupBy(ml => 1)
                .Select(g => new VaccineLotStatisticsResponseDTO
                {
                    TotalLots = g.Count(),
                    ActiveLots = g.Count(ml => !ml.IsDeleted && ml.ExpiryDate.Date > currentDate),
                    ExpiredLots = g.Count(ml => !ml.IsDeleted && ml.ExpiryDate.Date <= currentDate),
                    ExpiringInNext30Days = g.Count(ml => !ml.IsDeleted &&
                        ml.ExpiryDate.Date <= expiryThreshold && ml.ExpiryDate.Date > currentDate),
                    DeletedLots = g.Count(ml => ml.IsDeleted),
                    TotalQuantity = g.Where(ml => !ml.IsDeleted && ml.ExpiryDate.Date > currentDate).Sum(ml => ml.Quantity)
                })
                .FirstOrDefaultAsync();

            return statistics ?? new VaccineLotStatisticsResponseDTO();
        }

        #endregion

        #region Private Helper Methods

        private Expression<Func<MedicationLot, bool>> BuildVaccineLotPredicate(
            string? searchTerm, Guid? vaccineTypeId, bool? isExpired)
        {
            var today = _currentTime.GetVietnamTime().Date;

            return ml => ml.Type == LotType.Vaccine &&
                        !ml.IsDeleted &&
                        (string.IsNullOrWhiteSpace(searchTerm) ||
                         ml.LotNumber.Contains(searchTerm) ||
                         ml.StorageLocation.Contains(searchTerm) ||
                         ml.VaccineType.Name.Contains(searchTerm)) &&
                        (!vaccineTypeId.HasValue || ml.VaccineTypeId == vaccineTypeId) &&
                        (!isExpired.HasValue ||
                         (isExpired.Value && ml.ExpiryDate.Date <= today) ||
                         (!isExpired.Value && ml.ExpiryDate.Date > today));
        }

        private Expression<Func<MedicationLot, bool>> BuildLotsByVaccineTypePredicate(Guid vaccineTypeId)
        {
            return ml => ml.VaccineTypeId == vaccineTypeId &&
                        !ml.IsDeleted &&
                        ml.Type == LotType.Vaccine;
        }

        private Expression<Func<MedicationLot, bool>> BuildExpiringVaccineLotsPredicate(int daysBeforeExpiry)
        {
            var today = _currentTime.GetVietnamTime().Date;
            var thresholdDate = today.AddDays(daysBeforeExpiry);

            return ml => !ml.IsDeleted &&
                        ml.Type == LotType.Vaccine &&
                        ml.ExpiryDate.Date <= thresholdDate &&
                        ml.ExpiryDate.Date > today &&
                        ml.Quantity > 0;
        }

        private Expression<Func<MedicationLot, bool>> BuildExpiredVaccineLotsPredicate()
        {
            var today = _currentTime.GetVietnamTime().Date;
            return ml => !ml.IsDeleted &&
                        ml.Type == LotType.Vaccine &&
                        ml.ExpiryDate.Date <= today;
        }

        private Expression<Func<MedicationLot, bool>> BuildVaccineLotNumberExistsPredicate(string lotNumber, Guid? excludeId)
        {
            return ml => ml.LotNumber.ToLower() == lotNumber.ToLower() &&
                        !ml.IsDeleted &&
                        ml.Type == LotType.Vaccine &&
                        (!excludeId.HasValue || ml.Id != excludeId.Value);
        }

        private Expression<Func<MedicationLot, bool>> BuildSoftDeletedVaccinePredicate(string? searchTerm)
        {
            return ml => ml.IsDeleted &&
                        ml.Type == LotType.Vaccine &&
                        (string.IsNullOrWhiteSpace(searchTerm) ||
                         ml.LotNumber.Contains(searchTerm) ||
                         ml.StorageLocation.Contains(searchTerm) ||
                         ml.VaccineType.Name.Contains(searchTerm));
        }
        private Expression<Func<MedicationLot, bool>> BuildVaccineLotPredicate(
            string? searchTerm,
            Guid? vaccineTypeId,
            bool? isExpired,
            int? daysBeforeExpiry,
            bool? isDeleted)
        {
            var today = _currentTime.GetVietnamTime().Date;
            DateTime? threshold = daysBeforeExpiry.HasValue
                ? today.AddDays(daysBeforeExpiry.Value)
                : (DateTime?)null;

            return ml =>
                ml.Type == LotType.Vaccine
                && (!isDeleted.HasValue || ml.IsDeleted == isDeleted.Value)
                && (!vaccineTypeId.HasValue || ml.VaccineTypeId == vaccineTypeId.Value)
                && (string.IsNullOrWhiteSpace(searchTerm)
                    || ml.LotNumber.Contains(searchTerm)
                    || ml.StorageLocation.Contains(searchTerm)
                    || ml.VaccineType.Name.Contains(searchTerm))
                && (!isExpired.HasValue
                    || (isExpired.Value ? ml.ExpiryDate.Date <= today : ml.ExpiryDate.Date > today))
                && (!threshold.HasValue
                    || (ml.ExpiryDate.Date > today && ml.ExpiryDate.Date <= threshold.Value));
        }

        #endregion
    }
}