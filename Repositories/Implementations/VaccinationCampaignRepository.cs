using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.Implementations
{
    public class VaccinationCampaignRepository : GenericRepository<VaccinationCampaign, Guid>, IVaccinationCampaignRepository
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ICurrentTime _currentTime;

        public VaccinationCampaignRepository(SchoolHealthManagerDbContext context, ICurrentTime currentTime)
            : base(context)
        {
            _dbContext = context;
            _currentTime = currentTime;
        }

        public async Task<PagedList<VaccinationCampaign>> GetVaccinationCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            VaccinationCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var predicate = BuildCampaignPredicate(searchTerm, status, startDate, endDate);

            return await GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                orderBy: q => q.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.Name),
                includes: c => c.Schedules
            );
        }

        public async Task<VaccinationCampaign?> GetVaccinationCampaignByIdAsync(Guid id)
        {
            return await FirstOrDefaultAsync(
                c => c.Id == id && !c.IsDeleted,
                c => c.Schedules
            );
        }

        public async Task<VaccinationCampaign?> GetVaccinationCampaignWithDetailsAsync(Guid id)
        {
            return await _dbContext.VaccinationCampaigns
                .Include(c => c.Schedules)
                    .ThenInclude(s => s.VaccinationType)
                .Include(c => c.Schedules)
                    .ThenInclude(s => s.SessionStudents)
                .Include(c => c.Schedules)
                    .ThenInclude(s => s.Records)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<List<VaccinationCampaign>> GetVaccinationCampaignsByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            var query = includeDeleted
                ? _dbContext.VaccinationCampaigns.IgnoreQueryFilters()
                : _dbContext.VaccinationCampaigns.AsQueryable();

            return await query
                .Include(c => c.Schedules)
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<PagedList<VaccinationCampaign>> GetSoftDeletedCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var baseQuery = _dbContext.VaccinationCampaigns
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                baseQuery = baseQuery.Where(c =>
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            var query = baseQuery
                .Include(c => c.Schedules)
                .OrderByDescending(c => c.DeletedAt);

            return await PagedList<VaccinationCampaign>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<int> SoftDeleteCampaignsAsync(List<Guid> ids, Guid deletedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _dbContext.VaccinationCampaigns
                .Where(c => ids.Contains(c.Id) && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.IsDeleted, true)
                    .SetProperty(c => c.DeletedAt, currentTime)
                    .SetProperty(c => c.DeletedBy, deletedBy)
                    .SetProperty(c => c.UpdatedAt, currentTime)
                    .SetProperty(c => c.UpdatedBy, deletedBy));
        }

        public async Task<int> RestoreCampaignsAsync(List<Guid> ids, Guid restoredBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _dbContext.VaccinationCampaigns
                .IgnoreQueryFilters()
                .Where(c => ids.Contains(c.Id) && c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.IsDeleted, false)
                    .SetProperty(c => c.DeletedAt, (DateTime?)null)
                    .SetProperty(c => c.DeletedBy, (Guid?)null)
                    .SetProperty(c => c.UpdatedAt, currentTime)
                    .SetProperty(c => c.UpdatedBy, restoredBy));
        }

        public async Task<bool> CampaignNameExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _dbContext.VaccinationCampaigns
                .Where(c => !c.IsDeleted && c.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> UpdateCampaignStatusAsync(Guid campaignId, VaccinationCampaignStatus status, Guid updatedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _dbContext.VaccinationCampaigns
                .Where(c => c.Id == campaignId && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Status, status)
                    .SetProperty(c => c.UpdatedAt, currentTime)
                    .SetProperty(c => c.UpdatedBy, updatedBy));
        }

        public async Task<int> BatchUpdateCampaignStatusAsync(List<Guid> campaignIds, VaccinationCampaignStatus status, Guid updatedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _dbContext.VaccinationCampaigns
                .Where(c => campaignIds.Contains(c.Id) && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Status, status)
                    .SetProperty(c => c.UpdatedAt, currentTime)
                    .SetProperty(c => c.UpdatedBy, updatedBy));
        }

        private Expression<Func<VaccinationCampaign, bool>> BuildCampaignPredicate(
            string? searchTerm, VaccinationCampaignStatus? status, DateTime? startDate, DateTime? endDate)
        {
            var predicate = PredicateBuilder.New<VaccinationCampaign>(c => !c.IsDeleted);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                predicate = predicate.And(c =>
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (status.HasValue)
            {
                predicate = predicate.And(c => c.Status == status.Value);
            }

            if (startDate.HasValue)
            {
                predicate = predicate.And(c => c.StartDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                predicate = predicate.And(c => c.EndDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            return predicate;
        }
    }
}