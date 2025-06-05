using Microsoft.EntityFrameworkCore;
using Repositories.WorkSeeds;

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
                .Where(m => !m.IsDeleted); // Chỉ lấy những thuốc chưa bị xóa

            // Áp dụng bộ lọc tìm kiếm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m => m.Name.Contains(searchTerm) ||
                                        m.DosageForm.Contains(searchTerm));
            }

            // Áp dụng bộ lọc category
            if (category.HasValue)
            {
                query = query.Where(m => m.Category == category.Value);
            }

            // Bao gồm thông tin lot để tính toán tổng số lượng
            query = query.Include(m => m.Lots.Where(lot => !lot.IsDeleted));

            // Sắp xếp theo tên
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

        // Override các phương thức từ GenericRepository để không tự động SaveChanges
        public new async Task<Medication> AddAsync(Medication entity)
        {
            var now = DateTime.UtcNow;
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            entity.IsDeleted = false;

            await _dbSet.AddAsync(entity);
            // Không gọi SaveChangesAsync ở đây, để UnitOfWork quản lý
            return entity;
        }

        public new async Task UpdateAsync(Medication entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            // Không gọi SaveChangesAsync ở đây, để UnitOfWork quản lý
        }

        // Soft delete thay vì hard delete
        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
                // Không gọi SaveChangesAsync ở đây, để UnitOfWork quản lý
            }
        }
    }
}