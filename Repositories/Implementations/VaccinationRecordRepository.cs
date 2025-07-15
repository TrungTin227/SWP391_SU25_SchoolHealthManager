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

        public async Task<PagedList<VaccinationRecord>> GetRecordsAsync(
            Guid? studentId, Guid? scheduleId, Guid? vaccineTypeId,
            DateTime? from, DateTime? to, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .AsSplitQuery() // ✅ Tối ưu performance
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.Student) // ✅ Student thông qua SessionStudent
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule) // ✅ Schedule thông qua SessionStudent
                        .ThenInclude(vs => vs.Campaign)
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.VaccinationType) // ✅ VaccinationType thông qua Schedule
                .Include(vr => vr.VaccinatedBy)
                .Include(vr => vr.VaccineLot)
                .Where(vr => !vr.IsDeleted);

            // ✅ Filter thông qua SessionStudent relationships
            if (studentId.HasValue)
                query = query.Where(vr => vr.SessionStudent.StudentId == studentId);
            if (scheduleId.HasValue)
                query = query.Where(vr => vr.SessionStudent.VaccinationScheduleId == scheduleId);
            if (vaccineTypeId.HasValue)
                query = query.Where(vr => vr.SessionStudent.VaccinationSchedule.VaccinationTypeId == vaccineTypeId);
            if (from.HasValue)
                query = query.Where(vr => vr.AdministeredDate >= from);
            if (to.HasValue)
                query = query.Where(vr => vr.AdministeredDate <= to);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(vr => vr.SessionStudent.Student.FullName.ToLower().Contains(term));
            }

            query = query.OrderByDescending(vr => vr.AdministeredDate);
            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<PagedList<VaccinationRecord>> GetRecordsByStudentAsync(
            Guid studentId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.Student)
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.Campaign)
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.VaccinationType)
                .Include(vr => vr.VaccinatedBy)
                .Where(vr => vr.SessionStudent.StudentId == studentId && !vr.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(vr =>
                    vr.SessionStudent.Student.FullName.ToLower().Contains(term) ||
                    vr.SessionStudent.VaccinationSchedule.VaccinationType.Name.ToLower().Contains(term) ||
                    vr.SessionStudent.VaccinationSchedule.Campaign.Name.ToLower().Contains(term)
                );
            }

            query = query.OrderByDescending(vr => vr.VaccinatedAt);
            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<PagedList<VaccinationRecord>> GetRecordsByScheduleAsync(
            Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.Student)
                .Include(vr => vr.VaccinatedBy)
                .Include(vr => vr.VaccineLot)
                .Where(vr => vr.SessionStudent.VaccinationScheduleId == scheduleId && !vr.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(vr => vr.SessionStudent.Student.FullName.ToLower().Contains(searchTerm.ToLower()));

            query = query.OrderByDescending(vr => vr.VaccinatedAt);
            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<List<VaccinationRecord>> GetRecordsByDateAsync(DateTime from, DateTime to, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.Student)
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule)
                .Where(vr => vr.AdministeredDate >= from && vr.AdministeredDate <= to && !vr.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(vr => vr.SessionStudent.Student.FullName.ToLower().Contains(searchTerm.ToLower()));

            return await query.OrderByDescending(vr => vr.AdministeredDate).ToListAsync();
        }

        public async Task<VaccinationRecord?> GetRecordWithDetailsAsync(Guid id)
        {
            return await _context.VaccinationRecords
                .AsSplitQuery()
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.Student)
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.Campaign)
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule)
                        .ThenInclude(vs => vs.VaccinationType)
                .Include(vr => vr.VaccineLot)
                .Include(vr => vr.VaccinatedBy)
                .Include(vr => vr.CounselingAppointments)
                .Include(vr => vr.HealthEvents)
                .FirstOrDefaultAsync(vr => vr.Id == id && !vr.IsDeleted);
        }

        public async Task<List<VaccinationRecord>> GetRecordsByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            var query = _context.VaccinationRecords
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.Student)
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.VaccinationSchedule)
                .Include(vr => vr.VaccinatedBy)
                .Where(vr => ids.Contains(vr.Id));

            if (!includeDeleted)
                query = query.Where(vr => !vr.IsDeleted);

            return await query.ToListAsync();
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

        // ✅ Cập nhật HasDuplicateEntryAsync
        public async Task<bool> HasDuplicateEntryAsync(Guid studentId, Guid scheduleId)
        {
            return await _context.VaccinationRecords
                .AnyAsync(vr => vr.SessionStudent.StudentId == studentId &&
                              vr.SessionStudent.VaccinationScheduleId == scheduleId &&
                              !vr.IsDeleted);
        }

        public async Task<PagedList<VaccinationRecord>> GetSoftDeletedRecordsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.VaccinationRecords
                .IgnoreQueryFilters()
                .Include(vr => vr.SessionStudent)
                    .ThenInclude(ss => ss.Student)
                .Where(vr => vr.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(vr => vr.SessionStudent.Student.FullName.ToLower().Contains(term));
            }

            query = query.OrderByDescending(vr => vr.DeletedAt);
            return await PagedList<VaccinationRecord>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<bool> RestoreRecordAsync(Guid id, Guid restoredBy)
        {
            var record = await _context.VaccinationRecords
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(vr => vr.Id == id && vr.IsDeleted);

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
                .Where(vr => ids.Contains(vr.Id) && vr.IsDeleted)
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

        public async Task<MedicationLot?> GetAvailableLotByVaccineTypeAsync(Guid vaccineTypeId)
        {
            return await _context.MedicationLots
                .Where(l => l.VaccineTypeId == vaccineTypeId && l.Quantity > 0)
                .OrderBy(l => l.ExpiryDate) // Ưu tiên lô gần hết hạn
                .FirstOrDefaultAsync();
        }
        public async Task UpdateVaccineLotAsync(MedicationLot lot)
        {
            _context.MedicationLots.Update(lot);
            await _context.SaveChangesAsync();
        }
    }
}