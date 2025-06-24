using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.Implementations
{
    public class CheckupCampaignRepository : GenericRepository<CheckupCampaign, Guid>, ICheckupCampaignRepository
    {
        private readonly ICurrentTime _currentTime;

        public CheckupCampaignRepository(
            SchoolHealthManagerDbContext dbContext,
            ICurrentTime currentTime) : base(dbContext)
        {
            _currentTime = currentTime;
        }

        public async Task<CheckupCampaign?> GetCheckupCampaignByIdAsync(Guid id)
        {
            return await _context.CheckupCampaigns
                .Where(c => c.Id == id && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<CheckupCampaign?> GetCheckupCampaignWithDetailsAsync(Guid id)
        {
            return await _context.CheckupCampaigns
                .Include(c => c.Schedules.Where(s => !s.IsDeleted))
                    .ThenInclude(s => s.Student)
                .Include(c => c.Schedules.Where(s => !s.IsDeleted))
                    .ThenInclude(s => s.Record)
                .Where(c => c.Id == id && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedList<CheckupCampaign>> GetCheckupCampaignsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            CheckupCampaignStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var predicate = BuildCampaignPredicate(searchTerm, status, startDate, endDate);

            var query = _context.CheckupCampaigns
                .Where(predicate)
                .OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<CheckupCampaign>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<bool> CampaignNameExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.CheckupCampaigns
                .Where(c => !c.IsDeleted && c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> UpdateCampaignStatusAsync(Guid campaignId, CheckupCampaignStatus status, Guid updatedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _context.CheckupCampaigns
                .Where(c => c.Id == campaignId && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Status, status)
                    .SetProperty(c => c.UpdatedAt, currentTime)
                    .SetProperty(c => c.UpdatedBy, updatedBy));
        }

        public async Task<int> BatchUpdateCampaignStatusAsync(List<Guid> campaignIds, CheckupCampaignStatus status, Guid updatedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _context.CheckupCampaigns
                .Where(c => campaignIds.Contains(c.Id) && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Status, status)
                    .SetProperty(c => c.UpdatedAt, currentTime)
                    .SetProperty(c => c.UpdatedBy, updatedBy));
        }

        public async Task<int> BatchSoftDeleteAsync(List<Guid> campaignIds, Guid deletedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _context.CheckupCampaigns
                .Where(c => campaignIds.Contains(c.Id) && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.IsDeleted, true)
                    .SetProperty(c => c.DeletedAt, currentTime)
                    .SetProperty(c => c.DeletedBy, deletedBy));
        }

        public async Task<int> BatchRestoreAsync(List<Guid> campaignIds, Guid restoredBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _context.CheckupCampaigns
                .Where(c => campaignIds.Contains(c.Id) && c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.IsDeleted, false)
                    .SetProperty(c => c.DeletedAt, (DateTime?)null)
                    .SetProperty(c => c.DeletedBy, (Guid?)null)
                    .SetProperty(c => c.UpdatedAt, currentTime)
                    .SetProperty(c => c.UpdatedBy, restoredBy));
        }

        public async Task<Dictionary<CheckupCampaignStatus, int>> GetCampaignStatusCountsAsync()
        {
            return await _context.CheckupCampaigns
                .Where(c => !c.IsDeleted)
                .GroupBy(c => c.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        private Expression<Func<CheckupCampaign, bool>> BuildCampaignPredicate(
            string? searchTerm, CheckupCampaignStatus? status, DateTime? startDate, DateTime? endDate)
        {
            Expression<Func<CheckupCampaign, bool>> predicate = c => !c.IsDeleted;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                predicate = predicate.And(c =>
                    c.Name.Contains(searchTerm) ||
                    c.SchoolYear.Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(searchTerm)));
            }

            if (status.HasValue)
            {
                predicate = predicate.And(c => c.Status == status.Value);
            }

            if (startDate.HasValue)
            {
                predicate = predicate.And(c => c.ScheduledDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                predicate = predicate.And(c => c.ScheduledDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            return predicate;
        }
    }
}