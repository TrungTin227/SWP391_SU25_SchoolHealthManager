using DTOs.ParentVaccinationDTOs.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Implementations
{
    public class ParentVaccinationRepository : GenericRepository<VaccinationSchedule, Guid>, IParentVaccinationRepository
    {
        private readonly ILogger<ParentVaccinationRepository> _logger;
        private readonly ICurrentTime _currentTime;

        public ParentVaccinationRepository(
            SchoolHealthManagerDbContext context,
            ILogger<ParentVaccinationRepository> logger,
            ICurrentTime currentTime) : base(context)
        {
            _logger = logger;
            _currentTime = currentTime;
        }

        #region Query Operations

        public async Task<PagedList<VaccinationSchedule>> GetParentVaccinationSchedulesAsync(
            Guid parentUserId, ParentActionStatus? status, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting vaccination schedules for parent {ParentUserId} with status {Status}. Page: {PageNumber}, Size: {PageSize}",
                    parentUserId, status, pageNumber, pageSize);

                var query = _context.VaccinationSchedules
                    .Include(vs => vs.Campaign)
                    .Include(vs => vs.VaccinationType)
                    .Include(vs => vs.SessionStudents.Where(ss => ss.Student.ParentUserId == parentUserId))
                        .ThenInclude(ss => ss.Student)
                    .Include(vs => vs.Records.Where(vr => vr.Student.ParentUserId == parentUserId))
                    .Where(vs => !vs.IsDeleted &&
                               vs.SessionStudents.Any(ss => ss.Student.ParentUserId == parentUserId))
                    .AsQueryable();

                // Apply status filter
                if (status.HasValue)
                {
                    query = ApplyStatusFilter(query, status.Value, parentUserId);
                }

                // Apply ordering
                query = query.OrderBy(vs => vs.ScheduledAt);

                return await PagedList<VaccinationSchedule>.ToPagedListAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vaccination schedules for parent {ParentUserId} with status {Status}",
                    parentUserId, status);
                throw;
            }
        }

        public async Task<List<SessionStudent>> GetParentSessionStudentsAsync(Guid parentUserId, Guid scheduleId)
        {
            try
            {
                _logger.LogInformation("Getting session students for parent {ParentUserId} and schedule {ScheduleId}",
                    parentUserId, scheduleId);

                return await _context.SessionStudents
                    .Include(ss => ss.Student)
                    .Include(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.Campaign)
                    .Include(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.VaccinationType)
                    .Include(ss => ss.VaccinationRecords)
                    .Where(ss => ss.Student.ParentUserId == parentUserId &&
                               ss.VaccinationScheduleId == scheduleId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session students for parent {ParentUserId} and schedule {ScheduleId}",
                    parentUserId, scheduleId);
                throw;
            }
        }

        public async Task<List<VaccinationRecord>> GetParentVaccinationHistoryAsync(Guid parentUserId)
        {
            try
            {
                _logger.LogInformation("Getting vaccination history for parent {ParentUserId}", parentUserId);

                return await _context.VaccinationRecords
                    .Include(vr => vr.Student)
                    .Include(vr => vr.Schedule)
                        .ThenInclude(vs => vs.Campaign)
                    .Include(vr => vr.VaccineType)
                    .Include(vr => vr.VaccinatedBy)
                    .Include(vr => vr.VaccineLot)
                    .Where(vr => vr.Student.ParentUserId == parentUserId)
                    .OrderByDescending(vr => vr.VaccinatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vaccination history for parent {ParentUserId}", parentUserId);
                throw;
            }
        }

        public async Task<List<VaccinationRecord>> GetStudentVaccinationHistoryAsync(Guid parentUserId, Guid studentId)
        {
            try
            {
                _logger.LogInformation("Getting vaccination history for parent {ParentUserId} and student {StudentId}",
                    parentUserId, studentId);

                return await _context.VaccinationRecords
                    .Include(vr => vr.Student)
                    .Include(vr => vr.Schedule)
                        .ThenInclude(vs => vs.Campaign)
                    .Include(vr => vr.VaccineType)
                    .Include(vr => vr.VaccinatedBy)
                    .Include(vr => vr.VaccineLot)
                    .Where(vr => vr.Student.ParentUserId == parentUserId &&
                               vr.StudentId == studentId)
                    .OrderByDescending(vr => vr.VaccinatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vaccination history for parent {ParentUserId} and student {StudentId}",
                    parentUserId, studentId);
                throw;
            }
        }

        public async Task<List<VaccinationRecord>> GetFollowUpVaccinationsAsync(Guid parentUserId)
        {
            try
            {
                _logger.LogInformation("Getting follow-up vaccinations for parent {ParentUserId}", parentUserId);

                return await _context.VaccinationRecords
                    .Include(vr => vr.Student)
                    .Include(vr => vr.Schedule)
                        .ThenInclude(vs => vs.Campaign)
                    .Include(vr => vr.VaccineType)
                    .Where(vr => vr.Student.ParentUserId == parentUserId &&
                               vr.ReactionSeverity > VaccinationReactionSeverity.None)
                    .OrderByDescending(vr => vr.VaccinatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follow-up vaccinations for parent {ParentUserId}", parentUserId);
                throw;
            }
        }

        #endregion

        #region Statistics Operations

        public async Task<Dictionary<ParentActionStatus, int>> GetParentVaccinationStatsAsync(Guid parentUserId)
        {
            try
            {
                _logger.LogInformation("Getting vaccination statistics for parent {ParentUserId}", parentUserId);

                var stats = new Dictionary<ParentActionStatus, int>();

                // Count different statuses
                var pendingCount = await _context.SessionStudents
                    .CountAsync(ss => ss.Student.ParentUserId == parentUserId &&
                                    (ss.ConsentStatus == ParentConsentStatus.Pending ||
                                     ss.ConsentStatus == ParentConsentStatus.Sent));

                var approvedCount = await _context.SessionStudents
                    .CountAsync(ss => ss.Student.ParentUserId == parentUserId &&
                                    ss.ConsentStatus == ParentConsentStatus.Approved &&
                                    ss.VaccinationSchedule.ScheduleStatus == ScheduleStatus.Pending);

                var completedCount = await _context.VaccinationRecords
                    .CountAsync(vr => vr.Student.ParentUserId == parentUserId);

                var followUpCount = await _context.VaccinationRecords
                    .CountAsync(vr => vr.Student.ParentUserId == parentUserId &&
                                    vr.ReactionSeverity > VaccinationReactionSeverity.None);

                stats[ParentActionStatus.PendingConsent] = pendingCount;
                stats[ParentActionStatus.Approved] = approvedCount;
                stats[ParentActionStatus.Completed] = completedCount;
                stats[ParentActionStatus.RequiresFollowUp] = followUpCount;

                _logger.LogInformation("Retrieved vaccination statistics for parent {ParentUserId}: Pending={Pending}, Approved={Approved}, Completed={Completed}, FollowUp={FollowUp}",
                    parentUserId, pendingCount, approvedCount, completedCount, followUpCount);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vaccination statistics for parent {ParentUserId}", parentUserId);
                throw;
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> CanParentAccessStudentAsync(Guid parentUserId, Guid studentId)
        {
            try
            {
                var canAccess = await _context.Students
                    .AnyAsync(s => s.Id == studentId &&
                                 s.ParentUserId == parentUserId &&
                                 !s.IsDeleted);

                if (!canAccess)
                {
                    _logger.LogWarning("Parent {ParentUserId} cannot access student {StudentId}", parentUserId, studentId);
                }

                return canAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking parent {ParentUserId} access to student {StudentId}",
                    parentUserId, studentId);
                throw;
            }
        }

        public async Task<bool> CanParentAccessSessionAsync(Guid parentUserId, Guid sessionStudentId)
        {
            try
            {
                var canAccess = await _context.SessionStudents
                    .AnyAsync(ss => ss.Id == sessionStudentId &&
                                  ss.Student.ParentUserId == parentUserId);

                if (!canAccess)
                {
                    _logger.LogWarning("Parent {ParentUserId} cannot access session student {SessionStudentId}",
                        parentUserId, sessionStudentId);
                }

                return canAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking parent {ParentUserId} access to session student {SessionStudentId}",
                    parentUserId, sessionStudentId);
                throw;
            }
        }

        #endregion

        #region Business Logic Operations

        public async Task<bool> UpdateConsentStatusAsync(Guid sessionStudentId, ParentConsentStatus consentStatus, Guid parentUserId)
        {
            try
            {
                _logger.LogInformation("Updating consent status for session student {SessionStudentId} to {ConsentStatus} by parent {ParentUserId}",
                    sessionStudentId, consentStatus, parentUserId);

                var sessionStudent = await _context.SessionStudents
                    .Include(ss => ss.Student)
                    .FirstOrDefaultAsync(ss => ss.Id == sessionStudentId &&
                                             ss.Student.ParentUserId == parentUserId);

                if (sessionStudent == null)
                {
                    _logger.LogWarning("Session student {SessionStudentId} not found or parent {ParentUserId} has no access",
                        sessionStudentId, parentUserId);
                    return false;
                }

                var oldStatus = sessionStudent.ConsentStatus;
                sessionStudent.ConsentStatus = consentStatus;
                sessionStudent.ParentSignedAt = _currentTime.GetVietnamTime();

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Consent status updated for session student {SessionStudentId}. Old: {OldStatus}, New: {NewStatus}",
                        sessionStudentId, oldStatus, consentStatus);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating consent status for session student {SessionStudentId} to {ConsentStatus}",
                    sessionStudentId, consentStatus);
                throw;
            }
        }

        public async Task<List<SessionStudent>> GetPendingConsentSessionsAsync(Guid parentUserId)
        {
            try
            {
                _logger.LogInformation("Getting pending consent sessions for parent {ParentUserId}", parentUserId);

                return await _context.SessionStudents
                    .Include(ss => ss.Student)
                    .Include(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.Campaign)
                    .Include(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.VaccinationType)
                    .Where(ss => ss.Student.ParentUserId == parentUserId &&
                               (ss.ConsentStatus == ParentConsentStatus.Pending ||
                                ss.ConsentStatus == ParentConsentStatus.Sent))
                    .OrderBy(ss => ss.VaccinationSchedule.ScheduledAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending consent sessions for parent {ParentUserId}", parentUserId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private IQueryable<VaccinationSchedule> ApplyStatusFilter(IQueryable<VaccinationSchedule> query, ParentActionStatus status, Guid parentUserId)
        {
            return status switch
            {
                ParentActionStatus.PendingConsent => query.Where(vs =>
                    vs.SessionStudents.Any(ss => ss.Student.ParentUserId == parentUserId &&
                                                (ss.ConsentStatus == ParentConsentStatus.Pending ||
                                                 ss.ConsentStatus == ParentConsentStatus.Sent))),

                ParentActionStatus.Approved => query.Where(vs =>
                    vs.SessionStudents.Any(ss => ss.Student.ParentUserId == parentUserId &&
                                                ss.ConsentStatus == ParentConsentStatus.Approved &&
                                                vs.ScheduleStatus == ScheduleStatus.Pending)),

                ParentActionStatus.Completed => query.Where(vs =>
                    vs.SessionStudents.Any(ss => ss.Student.ParentUserId == parentUserId &&
                                                vs.ScheduleStatus == ScheduleStatus.Completed)),

                ParentActionStatus.RequiresFollowUp => query.Where(vs =>
                    vs.Records.Any(vr => vr.Student.ParentUserId == parentUserId &&
                                       vr.ReactionSeverity > VaccinationReactionSeverity.None)),

                _ => query
            };
        }

        #endregion
    }
}