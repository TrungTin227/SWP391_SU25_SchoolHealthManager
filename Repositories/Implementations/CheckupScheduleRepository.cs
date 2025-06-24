using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementations
{
    public class CheckupScheduleRepository : GenericRepository<CheckupSchedule, Guid>, ICheckupScheduleRepository
    {
        public CheckupScheduleRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }

        public async Task<CheckupSchedule?> GetCheckupScheduleByIdAsync(Guid id)
        {
            return await _context.CheckupSchedules
                .Include(cs => cs.Campaign)
                .Include(cs => cs.Student)
                .Include(cs => cs.Record)
                .FirstOrDefaultAsync(cs => cs.Id == id && !cs.IsDeleted);
        }

        public async Task<PagedList<CheckupSchedule>> GetCheckupSchedulesAsync(
            int pageNumber, int pageSize, Guid? campaignId = null,
            CheckupScheduleStatus? status = null, string? searchTerm = null)
        {
            var query = _context.CheckupSchedules
                .Include(cs => cs.Campaign)
                .Include(cs => cs.Student)
                .Where(cs => !cs.IsDeleted);

            // Filter by campaign
            if (campaignId.HasValue)
            {
                query = query.Where(cs => cs.CampaignId == campaignId.Value);
            }

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(cs => cs.ParentConsentStatus == status.Value);
            }

            // Filter by search term (student name or student code)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(cs =>
                    (cs.Student != null && cs.Student.FullName.ToLower().Contains(searchLower)) ||
                    (cs.Student != null && cs.Student.StudentCode.ToLower().Contains(searchLower)) ||
                    (cs.Campaign != null && cs.Campaign.Name.ToLower().Contains(searchLower)));
            }

            // Order by scheduled time (newest first)
            query = query.OrderByDescending(cs => cs.ScheduledAt);

            return await PagedList<CheckupSchedule>.ToPagedListAsync(query, pageNumber, pageSize);
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
                // Get current UTC time for consistency
                var utcNow = new DateTime(2025, 6, 24, 12, 28, 57, DateTimeKind.Utc);

                // Validate schedules before adding
                foreach (var schedule in schedules)
                {
                    // Ensure required fields are set
                    if (schedule.Id == Guid.Empty)
                        schedule.Id = Guid.NewGuid();

                    if (schedule.CreatedAt == default)
                        schedule.CreatedAt = utcNow;

                    if (schedule.UpdatedAt == default)
                        schedule.UpdatedAt = utcNow;

                    // Set default status if not specified
                    if (schedule.ParentConsentStatus == default)
                        schedule.ParentConsentStatus = CheckupScheduleStatus.Pending;

                    // Ensure IsDeleted is false for new records
                    schedule.IsDeleted = false;
                }

                await _context.CheckupSchedules.AddRangeAsync(schedules);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if logger is available
                // _logger?.LogError(ex, "Error in BatchCreateSchedulesAsync with {Count} schedules", schedules.Count);
                throw new InvalidOperationException($"Failed to batch create {schedules.Count} schedules", ex);
            }
        }

        public async Task<int> BatchUpdateScheduleStatusAsync(List<Guid> scheduleIds, CheckupScheduleStatus status, Guid updatedBy)
        {
            if (scheduleIds == null || !scheduleIds.Any())
                return 0;

            if (updatedBy == Guid.Empty)
                throw new ArgumentException("UpdatedBy cannot be empty", nameof(updatedBy));

            try
            {
                var schedules = await _context.CheckupSchedules
                    .Where(cs => scheduleIds.Contains(cs.Id) && !cs.IsDeleted)
                    .ToListAsync();

                if (!schedules.Any())
                    return 0;

                var updateTime = new DateTime(2025, 6, 24, 12, 28, 57, DateTimeKind.Utc);

                foreach (var schedule in schedules)
                {
                    schedule.ParentConsentStatus = status;
                    schedule.UpdatedAt = updateTime;
                    schedule.UpdatedBy = updatedBy;

                    // Set ConsentReceivedAt when status changes to Approved
                    if (status == CheckupScheduleStatus.Approved && !schedule.ConsentReceivedAt.HasValue)
                    {
                        schedule.ConsentReceivedAt = updateTime;
                    }
                    // Clear ConsentReceivedAt when status changes away from Approved
                    else if (status != CheckupScheduleStatus.Approved && schedule.ConsentReceivedAt.HasValue)
                    {
                        schedule.ConsentReceivedAt = null;
                    }
                }

                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if logger is available
                // _logger?.LogError(ex, "Error in BatchUpdateScheduleStatusAsync for {Count} schedules", scheduleIds.Count);
                throw new InvalidOperationException($"Failed to batch update status for {scheduleIds.Count} schedules", ex);
            }
        }

        public async Task<int> GetScheduleCountByCampaignAsync(Guid campaignId)
        {
            if (campaignId == Guid.Empty)
                throw new ArgumentException("CampaignId cannot be empty", nameof(campaignId));

            return await _context.CheckupSchedules
                .Where(cs => cs.CampaignId == campaignId && !cs.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetCompletedScheduleCountByCampaignAsync(Guid campaignId)
        {
            if (campaignId == Guid.Empty)
                throw new ArgumentException("CampaignId cannot be empty", nameof(campaignId));

            return await _context.CheckupSchedules
                .Where(cs => cs.CampaignId == campaignId &&
                           !cs.IsDeleted &&
                           cs.ParentConsentStatus == CheckupScheduleStatus.Completed)
                .CountAsync();
        }
    }
}