using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Implementations
{
    public class VaccinationScheduleRepository : GenericRepository<VaccinationSchedule, Guid>, IVaccinationScheduleRepository
    {
        private readonly ILogger<VaccinationScheduleRepository> _logger;
        private readonly ICurrentTime _currentTime;

        public VaccinationScheduleRepository(
            SchoolHealthManagerDbContext context,
            ILogger<VaccinationScheduleRepository> logger,
            ICurrentTime currentTime) : base(context)
        {
            _logger = logger;
            _currentTime = currentTime;
        }

        public async Task<PagedList<VaccinationSchedule>> GetSchedulesByCampaignAsync(
            Guid campaignId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationSchedules
                .Include(vs => vs.Campaign)
                .Include(vs => vs.VaccinationType)
                .Include(vs => vs.SessionStudents)
                    .ThenInclude(ss => ss.Student)
                .Include(vs => vs.Records)
                .Where(vs => vs.CampaignId == campaignId && !vs.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(vs =>
                    vs.VaccinationType.Name.ToLower().Contains(term) ||
                    vs.VaccinationType.Code.ToLower().Contains(term));
            }

            query = query.OrderBy(vs => vs.ScheduledAt)
                        .ThenBy(vs => vs.VaccinationType.Name);

            return await PagedList<VaccinationSchedule>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<PagedList<VaccinationSchedule>> GetSchedulesByDateRangeAsync(
            DateTime startDate, DateTime endDate, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationSchedules
                .Include(vs => vs.Campaign)
                .Include(vs => vs.VaccinationType)
                .Include(vs => vs.SessionStudents)
                    .ThenInclude(ss => ss.Student)
                .Include(vs => vs.Records)
                .Where(vs => vs.ScheduledAt >= startDate && vs.ScheduledAt <= endDate && !vs.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(vs =>
                    vs.Campaign.Name.ToLower().Contains(term) ||
                    vs.VaccinationType.Name.ToLower().Contains(term) ||
                    vs.VaccinationType.Code.ToLower().Contains(term));
            }

            query = query.OrderBy(vs => vs.ScheduledAt)
                        .ThenBy(vs => vs.Campaign.Name);

            return await PagedList<VaccinationSchedule>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<List<VaccinationSchedule>> GetSchedulesByStatusAsync(ScheduleStatus status)
        {
            return await _context.VaccinationSchedules
                .Include(vs => vs.Campaign)
                .Include(vs => vs.VaccinationType)
                .Where(vs => vs.ScheduleStatus == status && !vs.IsDeleted)
                .OrderBy(vs => vs.ScheduledAt)
                .ToListAsync();
        }

        public async Task<VaccinationSchedule?> GetScheduleWithDetailsAsync(Guid id)
        {
            return await _context.VaccinationSchedules
                .Include(vs => vs.Campaign)
                .Include(vs => vs.VaccinationType)
                .Include(vs => vs.SessionStudents)
                    .ThenInclude(ss => ss.Student)
                .Include(vs => vs.Records)
                    .ThenInclude(vr => vr.VaccinatedBy)
                .FirstOrDefaultAsync(vs => vs.Id == id && !vs.IsDeleted);
        }

        public async Task<List<VaccinationSchedule>> GetSchedulesByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            var query = _context.VaccinationSchedules
                .Include(vs => vs.Campaign)
                .Include(vs => vs.VaccinationType)
                .Include(vs => vs.SessionStudents)
                .Include(vs => vs.Records)
                .Where(vs => ids.Contains(vs.Id));

            if (!includeDeleted)
            {
                query = query.Where(vs => !vs.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> AddStudentsToScheduleAsync(Guid scheduleId, List<Guid> studentIds, Guid addedBy)
        {
            try
            {
                var schedule = await _context.VaccinationSchedules
                    .Include(vs => vs.SessionStudents)
                    .FirstOrDefaultAsync(vs => vs.Id == scheduleId && !vs.IsDeleted);

                if (schedule == null) return false;

                var existingStudentIds = schedule.SessionStudents.Select(ss => ss.StudentId).ToHashSet();
                var newStudentIds = studentIds.Where(id => !existingStudentIds.Contains(id)).ToList();

                if (!newStudentIds.Any()) return true;

                var now = _currentTime.GetVietnamTime();
                var sessionStudents = newStudentIds.Select(studentId => new SessionStudent
                {
                    Id = Guid.NewGuid(),
                    VaccinationScheduleId = scheduleId,
                    StudentId = studentId,
                    Status = SessionStatus.Registered,
                    CreatedAt = now,
                    CreatedBy = addedBy,
                    UpdatedAt = now,
                    UpdatedBy = addedBy
                }).ToList();

                _context.SessionStudents.AddRange(sessionStudents);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm học sinh vào lịch tiêm {ScheduleId}", scheduleId);
                return false;
            }
        }

        public async Task<bool> RemoveStudentsFromScheduleAsync(Guid scheduleId, List<Guid> studentIds)
        {
            try
            {
                var sessionStudents = await _context.SessionStudents
                    .Where(ss => ss.VaccinationScheduleId == scheduleId && studentIds.Contains(ss.StudentId))
                    .ToListAsync();

                if (!sessionStudents.Any()) return true;

                _context.SessionStudents.RemoveRange(sessionStudents);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa học sinh khỏi lịch tiêm {ScheduleId}", scheduleId);
                return false;
            }
        }

        public async Task<List<SessionStudent>> GetSessionStudentsByScheduleAsync(Guid scheduleId)
        {
            return await _context.SessionStudents
                .Include(ss => ss.Student)
                .Where(ss => ss.VaccinationScheduleId == scheduleId)
                .OrderBy(ss => ss.Student.FullName)
                .ToListAsync();
        }

        public async Task<bool> UpdateScheduleStatusAsync(Guid scheduleId, ScheduleStatus newStatus, Guid updatedBy)
        {
            try
            {
                var schedule = await _context.VaccinationSchedules
                    .FirstOrDefaultAsync(vs => vs.Id == scheduleId && !vs.IsDeleted);

                if (schedule == null) return false;

                schedule.ScheduleStatus = newStatus;
                schedule.UpdatedAt = _currentTime.GetVietnamTime();
                schedule.UpdatedBy = updatedBy;

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái lịch tiêm {ScheduleId}", scheduleId);
                return false;
            }
        }

        public async Task<bool> BatchUpdateScheduleStatusAsync(List<Guid> scheduleIds, ScheduleStatus newStatus, Guid updatedBy)
        {
            try
            {
                var schedules = await _context.VaccinationSchedules
                    .Where(vs => scheduleIds.Contains(vs.Id) && !vs.IsDeleted)
                    .ToListAsync();

                if (!schedules.Any()) return false;

                var now = _currentTime.GetVietnamTime();
                foreach (var schedule in schedules)
                {
                    schedule.ScheduleStatus = newStatus;
                    schedule.UpdatedAt = now;
                    schedule.UpdatedBy = updatedBy;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hàng loạt trạng thái lịch tiêm");
                return false;
            }
        }

        public async Task<PagedList<VaccinationSchedule>> GetSoftDeletedSchedulesAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationSchedules
                .IgnoreQueryFilters()
                .Include(vs => vs.Campaign)
                .Include(vs => vs.VaccinationType)
                .Where(vs => vs.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(vs =>
                    vs.Campaign.Name.ToLower().Contains(term) ||
                    vs.VaccinationType.Name.ToLower().Contains(term));
            }

            query = query.OrderByDescending(vs => vs.DeletedAt)
                        .ThenBy(vs => vs.ScheduledAt);

            return await PagedList<VaccinationSchedule>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<bool> RestoreScheduleAsync(Guid id, Guid restoredBy)
        {
            try
            {
                var schedule = await _context.VaccinationSchedules
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(vs => vs.Id == id && vs.IsDeleted);

                if (schedule == null) return false;

                schedule.IsDeleted = false;
                schedule.DeletedAt = null;
                schedule.DeletedBy = null;
                schedule.UpdatedAt = _currentTime.GetVietnamTime();
                schedule.UpdatedBy = restoredBy;

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khôi phục lịch tiêm {Id}", id);
                return false;
            }
        }

        public async Task<bool> BatchRestoreSchedulesAsync(List<Guid> ids, Guid restoredBy)
        {
            try
            {
                var schedules = await _context.VaccinationSchedules
                    .IgnoreQueryFilters()
                    .Where(vs => ids.Contains(vs.Id) && vs.IsDeleted)
                    .ToListAsync();

                if (!schedules.Any()) return false;

                var now = _currentTime.GetVietnamTime();
                foreach (var schedule in schedules)
                {
                    schedule.IsDeleted = false;
                    schedule.DeletedAt = null;
                    schedule.DeletedBy = null;
                    schedule.UpdatedAt = now;
                    schedule.UpdatedBy = restoredBy;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khôi phục hàng loạt lịch tiêm");
                return false;
            }
        }

        public async Task<bool> IsScheduleConflictAsync(Guid campaignId, DateTime scheduledAt, Guid? excludeScheduleId = null)
        {
            var query = _context.VaccinationSchedules
                .Where(vs => vs.CampaignId == campaignId &&
                           vs.ScheduledAt.Date == scheduledAt.Date &&
                           !vs.IsDeleted);

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(vs => vs.Id != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CanDeleteScheduleAsync(Guid id)
        {
            var hasRecords = await _context.VaccinationRecords
                .AnyAsync(vr => vr.ScheduleId == id);

            return !hasRecords;
        }
    }
}