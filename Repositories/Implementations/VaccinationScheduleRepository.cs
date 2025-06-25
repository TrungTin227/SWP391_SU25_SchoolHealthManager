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

        public async Task<PagedList<VaccinationSchedule>> GetSchedulesAsync(
           Guid? campaignId,
           DateTime? startDate,
           DateTime? endDate,
           ScheduleStatus? status,
           string? searchTerm,
           int pageNumber,
           int pageSize)
        {
            IQueryable<VaccinationSchedule> query = _context.VaccinationSchedules
                .Include(vs => vs.Campaign)
                .Include(vs => vs.VaccinationType)
                .Include(vs => vs.SessionStudents).ThenInclude(ss => ss.Student)
                .Include(vs => vs.Records)
                .Where(vs => !vs.IsDeleted);

            if (campaignId.HasValue)
                query = query.Where(vs => vs.CampaignId == campaignId.Value);
            if (startDate.HasValue)
                query = query.Where(vs => vs.ScheduledAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(vs => vs.ScheduledAt <= endDate.Value);
            if (status.HasValue)
                query = query.Where(vs => vs.ScheduleStatus == status.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(vs =>
                    vs.Campaign.Name.ToLower().Contains(term) ||
                    vs.VaccinationType.Name.ToLower().Contains(term) ||
                    vs.VaccinationType.Code.ToLower().Contains(term));
            }

            query = query.OrderBy(vs => vs.ScheduledAt)
                         .ThenBy(vs => vs.Campaign.Name);

            return await PagedList<VaccinationSchedule>.ToPagedListAsync(query, pageNumber, pageSize);
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

                // Kiểm tra studentIds có tồn tại trong database
                var validStudentIds = await _context.Students
                    .Where(s => studentIds.Contains(s.Id) && !s.IsDeleted)
                    .Select(s => s.Id)
                    .ToListAsync();

                //  Kiểm tra nếu có studentId không hợp lệ
                var invalidStudentIds = studentIds.Except(validStudentIds).ToList();
                if (invalidStudentIds.Any())
                {
                    _logger.LogWarning("Các studentId không hợp lệ: {InvalidIds}", string.Join(", ", invalidStudentIds));
                    // Tùy chọn: Có thể throw exception hoặc chỉ log warning
                    // throw new InvalidOperationException($"Các studentId không tồn tại: {string.Join(", ", invalidStudentIds)}");
                }

                var existingStudentIds = schedule.SessionStudents.Select(ss => ss.StudentId).ToHashSet();

                // Chỉ sử dụng studentIds hợp lệ và chưa tồn tại
                var newStudentIds = validStudentIds.Where(id => !existingStudentIds.Contains(id)).ToList();

                if (!newStudentIds.Any())
                {
                    // thông báo: Không có studentId hợp lệ nào để thêm
                    _logger.LogInformation("Không có studentId hợp lệ nào để thêm vào lịch tiêm {ScheduleId}", scheduleId);
                    return invalidStudentIds.Any() ? false : true; // Trả về false nếu có invalid IDs
                }

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

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Đã thêm {Count} học sinh hợp lệ vào lịch tiêm {ScheduleId}",
                        newStudentIds.Count, scheduleId);
                }

                return result;
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
            //  So sánh datetime chính xác, không chỉ ngày
            var query = _context.VaccinationSchedules
                .Where(vs => vs.CampaignId == campaignId &&
                           vs.ScheduledAt == scheduledAt &&  // So sánh chính xác datetime
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
        public async Task<bool> HasStudentScheduleConflictAsync(Guid studentId, DateTime scheduledAt, Guid? excludeScheduleId = null)
        {
            var timeWindow = TimeSpan.FromMinutes(60); // 1 hour buffer for vaccination
            var startTime = scheduledAt.Subtract(timeWindow);
            var endTime = scheduledAt.Add(timeWindow);

            var query = _context.VaccinationSchedules
                .Where(vs => !vs.IsDeleted &&
                           vs.ScheduledAt >= startTime &&
                           vs.ScheduledAt <= endTime &&
                           vs.SessionStudents.Any(ss => ss.StudentId == studentId && !ss.IsDeleted));

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(vs => vs.Id != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }
    }
}