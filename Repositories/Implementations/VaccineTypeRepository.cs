using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementations
{
    public class VaccineTypeRepository : GenericRepository<VaccinationType, Guid>, IVaccineTypeRepository
    {
        public VaccineTypeRepository(
            SchoolHealthManagerDbContext context) : base(context)
        {

        }

        public async Task<PagedList<VaccinationType>> GetVaccineTypesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<VaccinationType> query = _dbSet.AsQueryable()
                .Where(v => !v.IsDeleted)
                .Include(v => v.VaccineDoseInfos)
                .Include(v => v.Schedules)
                .Include(v => v.MedicationLots);

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(v =>
                    v.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    v.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    v.Group.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (isActive.HasValue)
            {
                query = query.Where(v => v.IsActive == isActive.Value);
            }

            // Order by
            query = query.OrderByDescending(v => v.CreatedAt)
                 .ThenBy(v => v.Name);

            return await PagedList<VaccinationType>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<VaccinationType?> GetVaccineTypeByIdAsync(Guid id)
        {
            return await _dbSet
                .Where(v => v.Id == id && !v.IsDeleted)
                .Include(v => v.VaccineDoseInfos)
                .Include(v => v.Schedules)
                .Include(v => v.MedicationLots)
                .FirstOrDefaultAsync();
        }

        public async Task<VaccinationType?> GetVaccineTypeWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Where(v => v.Id == id && !v.IsDeleted)
                .Include(v => v.VaccineDoseInfos.OrderBy(d => d.DoseNumber))
                .Include(v => v.Schedules)
                .Include(v => v.MedicationLots.Where(ml => !ml.IsDeleted))
                    .ThenInclude(ml => ml.Medication)
                .FirstOrDefaultAsync();
        }

        public async Task<VaccinationType?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Where(v => v.Code == code && !v.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<VaccinationType>> GetVaccineTypesByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            //Khai báo kiểu rõ ràng là IQueryable
            IQueryable<VaccinationType> query = _dbSet.AsQueryable()
                .Where(v => ids.Contains(v.Id))
                .Include(v => v.Schedules)
                .Include(v => v.MedicationLots);

            if (!includeDeleted)
            {
                query = query.Where(v => !v.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<PagedList<VaccinationType>> GetSoftDeletedVaccineTypesAsync(
     int pageNumber, int pageSize, string? searchTerm = null)
        {
            // Bước 1: Build base query
            var baseQuery = _dbSet.AsQueryable()
                .IgnoreQueryFilters()
                .Where(v => v.IsDeleted);

            // Bước 2: Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                baseQuery = baseQuery.Where(v =>
                    v.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    v.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    v.Group.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Bước 3: Apply includes và ordering
            var finalQuery = baseQuery
                .Include(v => v.VaccineDoseInfos)
                .Include(v => v.Schedules)
                .Include(v => v.MedicationLots)
                .OrderByDescending(v => v.DeletedAt)
                .ThenBy(v => v.Name);

            return await PagedList<VaccinationType>.ToPagedListAsync(finalQuery, pageNumber, pageSize);
        }

        public async Task<List<VaccinationType>> GetActiveVaccineTypesAsync()
        {
            return await _dbSet
                .Where(v => v.IsActive && !v.IsDeleted)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }
    }
}