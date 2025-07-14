using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.Implementations
{
    public class CheckupScheduleRepository : GenericRepository<CheckupSchedule, Guid>, ICheckupScheduleRepository
    {
        private readonly ICurrentTime _currentTime;

        public CheckupScheduleRepository(
            SchoolHealthManagerDbContext context,
            ICurrentTime currentTime) : base(context)
        {
            _currentTime = currentTime;
        }

        public async Task<CheckupSchedule?> GetCheckupScheduleByIdAsync(Guid id)
        {
            return await _context.CheckupSchedules
                .Include(cs => cs.Campaign)
                .Include(cs => cs.Student)
                .Include(cs => cs.Record)
                .FirstOrDefaultAsync(cs => cs.Id == id && !cs.IsDeleted);
        }

        public async Task<List<CheckupSchedule>> GetCheckupSchedulesByStudentIdAsync(Guid studentId)
        {
            return await _context.CheckupSchedules
                .Include(cs => cs.Campaign)
                .Include(cs => cs.Student)
                .Include(cs => cs.Record)
                .Where(cs => cs.StudentId == studentId && !cs.IsDeleted)
                .OrderByDescending(cs => cs.ScheduledAt)
                .ToListAsync();
        }



        public async Task<PagedList<CheckupSchedule>> GetCheckupSchedulesAsync(
            int pageNumber, int pageSize, Guid? campaignId = null,
            CheckupScheduleStatus? status = null, string? searchTerm = null)
        {
            var predicate = BuildSchedulePredicate(campaignId, status, searchTerm);

            var query = _context.CheckupSchedules
                .Include(cs => cs.Campaign)
                .Include(cs => cs.Student)
                .Include(cs => cs.Record)
                .Where(predicate)
                .OrderByDescending(cs => cs.ScheduledAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<CheckupSchedule>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<List<CheckupSchedule>> GetSchedulesByCampaignAsync(Guid campaignId)
        {
            return await _context.CheckupSchedules
                .Include(cs => cs.Student)
                .Include(cs => cs.Record)
                .Where(cs => cs.CampaignId == campaignId && !cs.IsDeleted)
                .OrderBy(cs => cs.ScheduledAt)
                .ToListAsync();
        }

        public async Task<int> BatchCreateSchedulesAsync(List<CheckupSchedule> schedules)
        {
            if (schedules == null || !schedules.Any())
                return 0;

            try
            {
                var currentTime = _currentTime.GetVietnamTime();

                foreach (var schedule in schedules)
                {
                    if (schedule.Id == Guid.Empty)
                        schedule.Id = Guid.NewGuid();

                    if (schedule.CreatedAt == default)
                        schedule.CreatedAt = currentTime;

                    if (schedule.UpdatedAt == default)
                        schedule.UpdatedAt = currentTime;

                    if (schedule.ParentConsentStatus == default)
                        schedule.ParentConsentStatus = CheckupScheduleStatus.Pending;

                    schedule.IsDeleted = false;
                    schedule.NotifiedAt = currentTime;
                }

                await _context.CheckupSchedules.AddRangeAsync(schedules);
                return await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> BatchUpdateScheduleStatusAsync(List<Guid> scheduleIds, CheckupScheduleStatus status, Guid updatedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _context.CheckupSchedules
                .Where(cs => scheduleIds.Contains(cs.Id) && !cs.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(cs => cs.ParentConsentStatus, status)
                    .SetProperty(cs => cs.UpdatedAt, currentTime)
                    .SetProperty(cs => cs.UpdatedBy, updatedBy)
                    .SetProperty(cs => cs.ConsentReceivedAt,
                        status == CheckupScheduleStatus.Approved || status == CheckupScheduleStatus.Declined
                            ? currentTime : (DateTime?)null));
        }

        public async Task<int> GetScheduleCountByCampaignAsync(Guid campaignId)
        {
            return await _context.CheckupSchedules
                .Where(cs => cs.CampaignId == campaignId && !cs.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetCompletedScheduleCountByCampaignAsync(Guid campaignId)
        {
            return await _context.CheckupSchedules
                .Where(cs => cs.CampaignId == campaignId &&
                           !cs.IsDeleted &&
                           cs.ParentConsentStatus == CheckupScheduleStatus.Completed)
                .CountAsync();
        }

        public async Task<int> BatchSoftDeleteAsync(List<Guid> scheduleIds, Guid deletedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _context.CheckupSchedules
                .Where(cs => scheduleIds.Contains(cs.Id) && !cs.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(cs => cs.IsDeleted, true)
                    .SetProperty(cs => cs.UpdatedAt, currentTime)
                    .SetProperty(cs => cs.UpdatedBy, deletedBy));
        }

        public async Task<int> BatchRestoreAsync(List<Guid> scheduleIds, Guid updatedBy)
        {
            var currentTime = _currentTime.GetVietnamTime();

            return await _context.CheckupSchedules
                .Where(cs => scheduleIds.Contains(cs.Id) && cs.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(cs => cs.IsDeleted, false)
                    .SetProperty(cs => cs.UpdatedAt, currentTime)
                    .SetProperty(cs => cs.UpdatedBy, updatedBy));
        }

        public async Task<bool> HasConflictingScheduleAsync(Guid studentId, DateTime scheduledAt, Guid? excludeId = null)
        {
            var timeWindow = TimeSpan.FromMinutes(30); // 30 minutes buffer
            var startTime = scheduledAt.Subtract(timeWindow);
            var endTime = scheduledAt.Add(timeWindow);

            var query = _context.CheckupSchedules
                .Where(cs => cs.StudentId == studentId &&
                           !cs.IsDeleted &&
                           cs.ScheduledAt >= startTime &&
                           cs.ScheduledAt <= endTime);

            if (excludeId.HasValue)
            {
                query = query.Where(cs => cs.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Dictionary<CheckupScheduleStatus, int>> GetScheduleStatusStatisticsAsync(Guid? campaignId = null)
        {
            var query = _context.CheckupSchedules.Where(cs => !cs.IsDeleted);

            if (campaignId.HasValue)
            {
                query = query.Where(cs => cs.CampaignId == campaignId.Value);
            }

            return await query
                .GroupBy(cs => cs.ParentConsentStatus)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        private Expression<Func<CheckupSchedule, bool>> BuildSchedulePredicate(
            Guid? campaignId, CheckupScheduleStatus? status, string? searchTerm)
        {
            var predicate = PredicateBuilder.True<CheckupSchedule>()
                .And(cs => !cs.IsDeleted);

            if (campaignId.HasValue)
            {
                predicate = predicate.And(cs => cs.CampaignId == campaignId.Value);
            }

            if (status.HasValue)
            {
                predicate = predicate.And(cs => cs.ParentConsentStatus == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                predicate = predicate.And(cs =>
                    (cs.Student != null && cs.Student.FullName.ToLower().Contains(searchLower)) ||
                    (cs.Student != null && cs.Student.StudentCode.ToLower().Contains(searchLower)) ||
                    (cs.Campaign != null && cs.Campaign.Name.ToLower().Contains(searchLower)));
            }

            return predicate;
        }
    }
}