using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Implementations
{
    public class VaccineDoseInfoRepository : GenericRepository<VaccineDoseInfo, Guid>, IVaccineDoseInfoRepository
    {

        public VaccineDoseInfoRepository(
            SchoolHealthManagerDbContext context
           ) : base(context)
        {
        }

        public async Task<PagedList<VaccineDoseInfo>> GetVaccineDoseInfosAsync(
                int pageNumber, int pageSize, Guid? vaccineTypeId = null, int? doseNumber = null)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (vaccineTypeId.HasValue)
            {
                query = query.Where(v => v.VaccineTypeId == vaccineTypeId.Value);
            }

            if (doseNumber.HasValue)
            {
                query = query.Where(v => v.DoseNumber == doseNumber.Value);
            }

            // Apply includes và ordering cuối cùng
            return await PagedList<VaccineDoseInfo>.ToPagedListAsync(
                query.Include(v => v.VaccineType)
                     .Include(v => v.PreviousDose)
                     .Include(v => v.NextDoses)
                     .OrderBy(v => v.VaccineType.Name)
                     .ThenBy(v => v.DoseNumber),
                pageNumber,
                pageSize);
        }

        public async Task<VaccineDoseInfo?> GetVaccineDoseInfoByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(v => v.VaccineType)
                .Include(v => v.PreviousDose)
                .Include(v => v.NextDoses)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<VaccineDoseInfo?> GetVaccineDoseInfoWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(v => v.VaccineType)
                .Include(v => v.PreviousDose)
                    .ThenInclude(p => p.VaccineType)
                .Include(v => v.NextDoses)
                    .ThenInclude(n => n.VaccineType)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<VaccineDoseInfo>> GetVaccineDoseInfosByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            var query = _dbSet.AsQueryable()
                .Where(v => ids.Contains(v.Id))
                .Include(v => v.VaccineType)
                .Include(v => v.PreviousDose)
                .Include(v => v.NextDoses);

            return await query.ToListAsync();
        }

        public async Task<List<VaccineDoseInfo>> GetDoseInfosByVaccineTypeAsync(Guid vaccineTypeId)
        {
            return await _dbSet
                .Where(v => v.VaccineTypeId == vaccineTypeId)
                .Include(v => v.PreviousDose)
                .Include(v => v.NextDoses)
                .OrderBy(v => v.DoseNumber)
                .ToListAsync();
        }

        public async Task<VaccineDoseInfo?> GetDoseInfoByVaccineTypeAndDoseNumberAsync(Guid vaccineTypeId, int doseNumber)
        {
            return await _dbSet
                .Include(v => v.VaccineType)
                .Include(v => v.PreviousDose)
                .Include(v => v.NextDoses)
                .FirstOrDefaultAsync(v => v.VaccineTypeId == vaccineTypeId && v.DoseNumber == doseNumber);
        }

        public async Task<bool> IsDoseNumberExistsAsync(Guid vaccineTypeId, int doseNumber, Guid? excludeId = null)
        {
            var query = _dbSet.Where(v => v.VaccineTypeId == vaccineTypeId && v.DoseNumber == doseNumber);

            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<VaccineDoseInfo>> GetNextDosesAsync(Guid currentDoseId)
        {
            return await _dbSet
                .Where(v => v.PreviousDoseId == currentDoseId)
                .Include(v => v.VaccineType)
                .OrderBy(v => v.DoseNumber)
                .ToListAsync();
        }

        public async Task<int> GetMaxDoseNumberByVaccineTypeAsync(Guid vaccineTypeId)
        {
            var maxDoseNumber = await _dbSet
                .Where(v => v.VaccineTypeId == vaccineTypeId)
                .MaxAsync(v => (int?)v.DoseNumber);

            return maxDoseNumber ?? 0;
        }
    }
}