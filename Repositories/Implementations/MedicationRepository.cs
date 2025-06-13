using Microsoft.EntityFrameworkCore;
using Repositories.WorkSeeds.Implements;
using System.Linq.Expressions;

namespace Repositories.Implementations
{
    public class MedicationRepository : GenericRepository<Medication, Guid>, IMedicationRepository
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ICurrentTime _currentTime;

        public MedicationRepository(SchoolHealthManagerDbContext context, ICurrentTime currentTime) : base(context)
        {
            _dbContext = context;
            _currentTime = currentTime;
        }

        #region Specific Business Logic Methods

        public async Task<PagedList<Medication>> GetMedicationsAsync(int pageNumber, int pageSize, string? searchTerm = null, MedicationCategory? category = null)
        {
            // Sử dụng predicate từ GenericRepository
            var predicate = BuildMedicationPredicate(searchTerm, category);

            return await GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                orderBy: q => q.OrderBy(m => m.Name),
                includes: m => m.Lots.Where(lot => !lot.IsDeleted)
            );
        }

        /// <summary>
        /// Lấy danh sách medications bao gồm cả những cái đã bị soft delete (for MedicationService.GetMedicationsAsync)
        /// </summary>
        public async Task<PagedList<Medication>> GetAllMedicationsIncludingDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null, MedicationCategory? category = null)
        {
            var predicate = BuildMedicationPredicateIncludingDeleted(searchTerm, category);

            // Sử dụng IgnoreQueryFilters để lấy cả soft deleted items
            IQueryable<Medication> query = _dbContext.Medications.IgnoreQueryFilters();

            if (predicate != null)
                query = query.Where(predicate);

            query = query.Include(m => m.Lots);
            query = query.OrderBy(m => m.Name);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<Medication>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<bool> MedicationNameExistsAsync(string name, Guid? excludeId = null)
        {
            var predicate = BuildNameExistsPredicate(name, excludeId);
            return await AnyAsync(predicate);
        }

        public async Task<List<Medication>> GetMedicationsByCategoryAsync(MedicationCategory category)
        {
            var medications = await GetAllAsync(
                predicate: m => m.Category == category && !m.IsDeleted,
                orderBy: q => q.OrderBy(m => m.Name)
            );
            return medications.ToList();
        }

        public async Task<List<Medication>> GetActiveMedicationsAsync()
        {
            var medications = await GetAllAsync(
                predicate: m => m.Status == MedicationStatus.Active && !m.IsDeleted,
                orderBy: q => q.OrderBy(m => m.Name)
            );
            return medications.ToList();
        }

        public async Task<int> GetTotalQuantityByMedicationIdAsync(Guid medicationId)
        {
            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(lot => lot.MedicationId == medicationId && !lot.IsDeleted)
                .SumAsync(lot => lot.Quantity);
        }

        /// <summary>
        /// Override GetByIdAsync to support includes parameter (for MedicationService.GetMedicationByIdAsync)
        /// </summary>
        public async Task<Medication?> GetByIdAsync<TProperty>(Guid id, Expression<Func<Medication, TProperty>> include)
        {
            return await _dbContext.Medications
                .Include(include)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        #endregion

        #region Extended Soft Delete Methods

        /// <summary>
        /// Soft delete medication và các lots liên quan
        /// </summary>
        public async Task<bool> SoftDeleteWithLotsAsync(Guid id, Guid deletedBy)
        {
            var medication = await GetByIdAsync(id);
            if (medication == null)
                return false;

            // Sử dụng soft delete từ base repository
            var result = await SoftDeleteAsync(id, deletedBy);
            if (!result) return false;

            // Soft delete all related lots
            var lots = await _dbContext.MedicationLots
                .Where(l => l.MedicationId == id && !l.IsDeleted)
                .ToListAsync();

            var now = _currentTime.GetVietnamTime();
            foreach (var lot in lots)
            {
                lot.IsDeleted = true;
                lot.DeletedAt = now;
                lot.DeletedBy = deletedBy;
                lot.UpdatedAt = now;
                lot.UpdatedBy = deletedBy;
            }

            return true;
        }

        /// <summary>
        /// Khôi phục medication đã bị soft delete
        /// </summary>
        public async Task<bool> RestoreWithLotsAsync(Guid id, Guid restoredBy)
        {
            var medication = await _dbContext.Medications
                .IgnoreQueryFilters() // To get soft deleted items
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null || !medication.IsDeleted)
                return false;

            // Restore medication using base repository method
            var result = await RestoreAsync(id, restoredBy);
            if (!result) return false;

            // Restore related lots that were deleted at the same time
            var lots = await _dbContext.MedicationLots
                .IgnoreQueryFilters()
                .Where(l => l.MedicationId == id && l.IsDeleted)
                .ToListAsync();

            var now = _currentTime.GetVietnamTime();
            foreach (var lot in lots)
            {
                lot.IsDeleted = false;
                lot.DeletedAt = null;
                lot.DeletedBy = null;
                lot.UpdatedAt = now;
                lot.UpdatedBy = restoredBy;
            }

            return true;
        }

        /// <summary>
        /// Xóa vĩnh viễn medication và tất cả lots liên quan
        /// </summary>
        public async Task<bool> PermanentDeleteWithLotsAsync(Guid id)
        {
            var medication = await _dbContext.Medications
                .IgnoreQueryFilters() // To get soft deleted items
                .Include(m => m.Lots)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
                return false;

            // Remove all related lots first
            _dbContext.MedicationLots.RemoveRange(medication.Lots);

            // Remove medication
            _dbContext.Medications.Remove(medication);

            return true;
        }

        /// <summary>
        /// Lấy danh sách các medication đã bị soft delete với phân trang
        /// </summary>
        public async Task<PagedList<Medication>> GetSoftDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var predicate = BuildSoftDeletedPredicate(searchTerm);

            // Sử dụng manual query vì cần IgnoreQueryFilters
            IQueryable<Medication> query = _dbContext.Medications.IgnoreQueryFilters();

            query = query.Where(predicate);
            query = query.Include(m => m.Lots);
            query = query.OrderByDescending(m => m.DeletedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<Medication>(items, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Xóa vĩnh viễn các medication đã soft delete quá thời hạn
        /// </summary>
        public async Task<int> PermanentDeleteExpiredAsync(int daysToExpire = 30)
        {
            var expiredDate = _currentTime.GetVietnamTime().AddDays(-daysToExpire);

            var expiredMedications = await _dbContext.Medications
                .IgnoreQueryFilters()
                .Include(m => m.Lots)
                .Where(m => m.IsDeleted &&
                           m.DeletedAt.HasValue &&
                           m.DeletedAt <= expiredDate)
                .ToListAsync();

            if (!expiredMedications.Any())
                return 0;

            var count = expiredMedications.Count;

            foreach (var medication in expiredMedications)
            {
                // Remove all related lots first
                _dbContext.MedicationLots.RemoveRange(medication.Lots);
                // Remove medication
                _dbContext.Medications.Remove(medication);
            }

            return count;
        }

        #endregion

        #region Private Helper Methods

        private Expression<Func<Medication, bool>> BuildMedicationPredicate(string? searchTerm, MedicationCategory? category)
        {
            return m => !m.IsDeleted &&
                       (string.IsNullOrWhiteSpace(searchTerm) ||
                        m.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        (m.DosageForm != null && m.DosageForm.ToLower().Contains(searchTerm.ToLower()))) &&
                       (!category.HasValue || m.Category == category.Value);
        }

        private Expression<Func<Medication, bool>> BuildMedicationPredicateIncludingDeleted(string? searchTerm, MedicationCategory? category)
        {
            return m => (string.IsNullOrWhiteSpace(searchTerm) ||
                        m.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        (m.DosageForm != null && m.DosageForm.ToLower().Contains(searchTerm.ToLower()))) &&
                       (!category.HasValue || m.Category == category.Value);
        }

        private Expression<Func<Medication, bool>> BuildNameExistsPredicate(string name, Guid? excludeId)
        {
            return m => m.Name.ToLower() == name.ToLower() &&
                       !m.IsDeleted &&
                       (!excludeId.HasValue || m.Id != excludeId.Value);
        }

        private Expression<Func<Medication, bool>> BuildSoftDeletedPredicate(string? searchTerm)
        {
            return m => m.IsDeleted &&
                       (string.IsNullOrWhiteSpace(searchTerm) ||
                        m.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        (m.DosageForm != null && m.DosageForm.ToLower().Contains(searchTerm.ToLower())));
        }

        #endregion
    }
}