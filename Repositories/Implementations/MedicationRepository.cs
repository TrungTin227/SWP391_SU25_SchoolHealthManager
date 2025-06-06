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

        public async Task<PagedList<Medication>> GetMedicationsAsync(int pageNumber, int pageSize, string? searchTerm = null, MedicationCategory? category = null)
        {
            var query = _dbContext.Medications
                .AsNoTracking()
                .Where(m => !m.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m => m.Name.Contains(searchTerm) ||
                                        m.DosageForm.Contains(searchTerm));
            }

            // Apply category filter
            if (category.HasValue)
            {
                query = query.Where(m => m.Category == category.Value);
            }

            // Include lots for quantity calculation
            query = query.Include(m => m.Lots.Where(lot => !lot.IsDeleted));

            // Order by name
            query = query.OrderBy(m => m.Name);

            return await PagedList<Medication>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<bool> MedicationNameExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _dbContext.Medications
                .AsNoTracking()
                .Where(m => m.Name.ToLower() == name.ToLower() && !m.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<Medication>> GetMedicationsByCategoryAsync(MedicationCategory category)
        {
            return await _dbContext.Medications
                .AsNoTracking()
                .Where(m => m.Category == category && !m.IsDeleted)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<Medication>> GetActiveMedicationsAsync()
        {
            return await _dbContext.Medications
                .AsNoTracking()
                .Where(m => m.Status == MedicationStatus.Active && !m.IsDeleted)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<int> GetTotalQuantityByMedicationIdAsync(Guid medicationId)
        {
            return await _dbContext.MedicationLots
                .AsNoTracking()
                .Where(lot => lot.MedicationId == medicationId && !lot.IsDeleted)
                .SumAsync(lot => lot.Quantity);
        }

        // Override methods from GenericRepository without auto SaveChanges
        public new async Task<Medication> AddAsync(Medication entity)
        {
            var now = DateTime.UtcNow;
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            entity.IsDeleted = false;

            await _dbSet.AddAsync(entity);
            return entity;
        }

        public new async Task UpdateAsync(Medication entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Soft delete medication và các lots liên quan
        /// </summary>
        public async Task<bool> SoftDeleteAsync(Guid id, Guid deletedBy)
        {
            var medication = await _dbContext.Medications.FindAsync(id);
            if (medication == null || medication.IsDeleted)
                return false;

            var now = DateTime.UtcNow;
            medication.IsDeleted = true;
            medication.DeletedAt = now;
            medication.DeletedBy = deletedBy;
            medication.UpdatedAt = now;
            medication.UpdatedBy = deletedBy;

            // Soft delete all related lots
            var lots = await _dbContext.MedicationLots
                .Where(l => l.MedicationId == id && !l.IsDeleted)
                .ToListAsync();

            foreach (var lot in lots)
            {
                lot.IsDeleted = true;
                lot.DeletedAt = now;
                lot.DeletedBy = deletedBy;
                lot.UpdatedAt = now;
                lot.UpdatedBy = deletedBy;
            }

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Khôi phục medication đã bị soft delete
        /// </summary>
        public async Task<bool> RestoreAsync(Guid id, Guid restoredBy)
        {
            var medication = await _dbContext.Medications.FindAsync(id);
            if (medication == null || !medication.IsDeleted)
                return false;

            medication.IsDeleted = false;
            medication.DeletedAt = null;
            medication.DeletedBy = null;
            medication.UpdatedAt = DateTime.UtcNow;
            medication.UpdatedBy = restoredBy;

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Xóa vĩnh viễn medication và tất cả lots liên quan
        /// </summary>
        public async Task<bool> PermanentDeleteAsync(Guid id)
        {
            var medication = await _dbContext.Medications
                .Include(m => m.Lots)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
                return false;

            // Remove all related lots first
            _dbContext.MedicationLots.RemoveRange(medication.Lots);
            _dbContext.Medications.Remove(medication);

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Lấy danh sách các medication đã bị soft delete với phân trang
        /// </summary>
        public async Task<PagedList<Medication>> GetSoftDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbContext.Medications
                .AsNoTracking()
                .Where(m => m.IsDeleted);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m => m.Name.Contains(searchTerm) ||
                                        m.DosageForm.Contains(searchTerm));
            }

            // Include all lots (both deleted and non-deleted for complete information)
            query = query.Include(m => m.Lots);

            // Order by deletion date (most recent first)
            query = query.OrderByDescending(m => m.DeletedAt);

            return await PagedList<Medication>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        /// <summary>
        /// Xóa vĩnh viễn các medication đã soft delete quá thời hạn
        /// </summary>
        public async Task<int> PermanentDeleteExpiredAsync(int daysToExpire = 30)
        {
            var expiredDate = DateTime.UtcNow.AddDays(-daysToExpire);

            var expiredMedications = await _dbContext.Medications
                .Where(m => m.IsDeleted && m.DeletedAt.HasValue && m.DeletedAt <= expiredDate)
                .Include(m => m.Lots)
                .ToListAsync();

            if (!expiredMedications.Any())
                return 0;

            var count = expiredMedications.Count;

            foreach (var medication in expiredMedications)
            {
                _dbContext.MedicationLots.RemoveRange(medication.Lots);
                _dbContext.Medications.Remove(medication);
            }

            await _dbContext.SaveChangesAsync();
            return count;
        }

        // Keep the original soft delete method for backward compatibility
        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
        }
    }
}