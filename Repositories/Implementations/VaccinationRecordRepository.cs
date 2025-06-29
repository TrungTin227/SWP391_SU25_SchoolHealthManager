using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Implementations
{
    public class VaccinationRecordRepository : GenericRepository<VaccinationRecord, Guid>, IVaccinationRecordRepository
    {
        private readonly ILogger<VaccinationRecordRepository> _logger;
        private readonly ICurrentTime _currentTime;

        public VaccinationRecordRepository(
            SchoolHealthManagerDbContext context,
            ILogger<VaccinationRecordRepository> logger,
            ICurrentTime currentTime) : base(context)
        {
            _logger = logger;
            _currentTime = currentTime;
        }

        public async Task<PagedList<VaccinationRecord>> GetRecordsAsync(Guid? studentId, Guid? scheduleId, Guid? vaccineTypeId, DateTime? from, DateTime? to, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .Include(vr => vr.Student)
                .Include(vr => vr.Schedule)
                .Include(vr => vr.VaccineType)
                .Include(vr => vr.VaccinatedBy)
                .Where(vr => !vr.IsDeleted);

            if (studentId.HasValue)
                query = query.Where(vr => vr.StudentId == studentId);
            if (scheduleId.HasValue)
                query = query.Where(vr => vr.ScheduleId == scheduleId);
            if (vaccineTypeId.HasValue)
                query = query.Where(vr => vr.VaccineTypeId == vaccineTypeId);
            if (from.HasValue)
                query = query.Where(vr => vr.AdministeredDate >= from);
            if (to.HasValue)
                query = query.Where(vr => vr.AdministeredDate <= to);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(vr => vr.Student.FullName.ToLower().Contains(term));
            }

            query = query.OrderByDescending(vr => vr.AdministeredDate);
            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<bool> UpdateReactionFollowUpAsync(Guid id, bool followup24h, bool followup72h)
        {
            try
            {
                var record = await _context.VaccinationRecords.FindAsync(id);
                if (record == null) return false;

                record.ReactionFollowup24h = followup24h;
                record.ReactionFollowup72h = followup72h;
                record.UpdatedAt = _currentTime.GetVietnamTime();

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reaction follow-up for record {RecordId}", id);
                return false;
            }
        }

        public async Task<bool> BatchUpdateReactionSeverityAsync(List<Guid> ids, VaccinationReactionSeverity severity)
        {
            try
            {
                var records = await _context.VaccinationRecords.Where(r => ids.Contains(r.Id)).ToListAsync();
                if (!records.Any()) return false;

                foreach (var record in records)
                {
                    record.ReactionSeverity = severity;
                    record.UpdatedAt = _currentTime.GetVietnamTime();
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch update reaction severity failed.");
                return false;
            }
        }

        public async Task<bool> HasDuplicateEntryAsync(Guid studentId, Guid scheduleId)
        {
            return await _context.VaccinationRecords
                .AnyAsync(r => r.StudentId == studentId && r.ScheduleId == scheduleId && !r.IsDeleted);
        }

        public async Task<PagedList<VaccinationRecord>> GetSoftDeletedRecordsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .IgnoreQueryFilters()
                .Where(r => r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(r => r.Student.FullName.ToLower().Contains(term));
            }

            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<bool> RestoreRecordAsync(Guid id, Guid restoredBy)
        {
            var record = await _context.VaccinationRecords.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted);
            if (record == null) return false;

            record.IsDeleted = false;
            record.DeletedAt = null;
            record.DeletedBy = null;
            record.UpdatedAt = _currentTime.GetVietnamTime();
            record.UpdatedBy = restoredBy;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> BatchRestoreRecordsAsync(List<Guid> ids, Guid restoredBy)
        {
            var records = await _context.VaccinationRecords
                .IgnoreQueryFilters()
                .Where(r => ids.Contains(r.Id) && r.IsDeleted)
                .ToListAsync();

            if (!records.Any()) return false;

            var now = _currentTime.GetVietnamTime();
            foreach (var record in records)
            {
                record.IsDeleted = false;
                record.DeletedAt = null;
                record.DeletedBy = null;
                record.UpdatedAt = now;
                record.UpdatedBy = restoredBy;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<VaccinationRecord?> GetRecordWithDetailsAsync(Guid id)
        {
            return await _context.VaccinationRecords
                .Include(r => r.Student)
                .Include(r => r.SessionStudent)
                .Include(r => r.Schedule)
                .Include(r => r.VaccineLot)
                .Include(r => r.VaccinatedBy)
                .Include(r => r.VaccineType)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<List<VaccinationRecord>> GetRecordsByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            var query = _context.VaccinationRecords.AsQueryable();

            if (!includeDeleted)
                query = query.Where(r => !r.IsDeleted);

            return await query.Where(r => ids.Contains(r.Id)).ToListAsync();
        }

        public async Task<PagedList<VaccinationRecord>> GetRecordsByStudentAsync(Guid studentId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .Include(r => r.Schedule)
                .Where(r => r.StudentId == studentId && !r.IsDeleted);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(r =>
                    r.Student.FullName.ToLower().Contains(term) ||
                    r.VaccineType.Name.ToLower().Contains(term) ||
                    r.Schedule.Campaign.Name.ToLower().Contains(term)
                );
            }

            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<PagedList<VaccinationRecord>> GetRecordsByScheduleAsync(Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .Include(r => r.Student)
                .Where(r => r.ScheduleId == scheduleId && !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(r => r.Student.FullName.ToLower().Contains(searchTerm.ToLower()));

            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<List<VaccinationRecord>> GetRecordsByDateAsync(DateTime from, DateTime to, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .Where(r => r.AdministeredDate >= from && r.AdministeredDate <= to && !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(r => r.Student.FullName.ToLower().Contains(searchTerm.ToLower()));

            return await query.ToListAsync();
        }

       
    }
}
