using Microsoft.EntityFrameworkCore;
using Repositories.WorkSeeds.Implements;

namespace Repositories.Implementations
{
    public class MedicationRepository : GenericRepository<Medication, Guid>, IMedicationRepository
    {
        private readonly SchoolHealthManagerDbContext _dbContext;

        public MedicationRepository(SchoolHealthManagerDbContext context) : base(context)
        {
            _dbContext = context;
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

        #endregion

        #region Extended Soft Delete Methods

        /// <summary>
        /// Soft delete medication và các lots liên quan
        /// </summary>
        public async Task<bool> SoftDeleteWithLotsAsync(Guid id, Guid deletedBy)
        {
            var medication = await GetByIdAsync(id);
            if (medication == null || medication.IsDeleted)
                return false;

            // Sử dụng soft delete từ base repository
            var result = await SoftDeleteAsync(id, deletedBy);
            if (!result) return false;

            // Soft delete all related lots
            var lots = await _dbContext.MedicationLots
                .Where(l => l.MedicationId == id && !l.IsDeleted)
                .ToListAsync();

            var now = DateTime.UtcNow;
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
            // Sử dụng restore từ base repository
            return await RestoreAsync(id, restoredBy);
        }

        /// <summary>
        /// Xóa vĩnh viễn medication và tất cả lots liên quan
        /// </summary>
        public async Task<bool> PermanentDeleteWithLotsAsync(Guid id)
        {
            var medication = await _dbContext.Medications
                .Include(m => m.Lots)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
                return false;

            // Remove all related lots first
            _dbContext.MedicationLots.RemoveRange(medication.Lots);

            // Sử dụng delete từ base repository
            return await DeleteAsync(id);
        }

        /// <summary>
        /// Lấy danh sách các medication đã bị soft delete với phân trang
        /// </summary>
        public async Task<PagedList<Medication>> GetSoftDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var predicate = BuildSoftDeletedPredicate(searchTerm);

            return await GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                orderBy: q => q.OrderByDescending(m => m.DeletedAt),
                includes: m => m.Lots
            );
        }

        /// <summary>
        /// Xóa vĩnh viễn các medication đã soft delete quá thời hạn
        /// </summary>
        public async Task<int> PermanentDeleteExpiredAsync(int daysToExpire = 30)
        {
            var expiredDate = DateTime.UtcNow.AddDays(-daysToExpire);
            var predicate = BuildExpiredPredicate(expiredDate);

            var expiredMedications = await GetAllAsync(
                predicate: predicate,
                includes: m => m.Lots
            );

            if (!expiredMedications.Any())
                return 0;

            var count = expiredMedications.Count;

            foreach (var medication in expiredMedications)
            {
                _dbContext.MedicationLots.RemoveRange(medication.Lots);
                await DeleteAsync(medication.Id);
            }

            return count;
        }

        #endregion

        #region Private Helper Methods

        private System.Linq.Expressions.Expression<Func<Medication, bool>> BuildMedicationPredicate(string? searchTerm, MedicationCategory? category)
        {
            return m => !m.IsDeleted &&
                       (string.IsNullOrWhiteSpace(searchTerm) ||
                        m.Name.Contains(searchTerm) ||
                        m.DosageForm.Contains(searchTerm)) &&
                       (!category.HasValue || m.Category == category.Value);
        }

        private System.Linq.Expressions.Expression<Func<Medication, bool>> BuildNameExistsPredicate(string name, Guid? excludeId)
        {
            return m => m.Name.ToLower() == name.ToLower() &&
                       !m.IsDeleted &&
                       (!excludeId.HasValue || m.Id != excludeId.Value);
        }

        private System.Linq.Expressions.Expression<Func<Medication, bool>> BuildSoftDeletedPredicate(string? searchTerm)
        {
            return m => m.IsDeleted &&
                       (string.IsNullOrWhiteSpace(searchTerm) ||
                        m.Name.Contains(searchTerm) ||
                        m.DosageForm.Contains(searchTerm));
        }

        private System.Linq.Expressions.Expression<Func<Medication, bool>> BuildExpiredPredicate(DateTime expiredDate)
        {
            return m => m.IsDeleted &&
                       m.DeletedAt.HasValue &&
                       m.DeletedAt <= expiredDate;
        }

        #endregion
    }
}