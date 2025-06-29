using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IVaccinationRecordRepository : IGenericRepository<VaccinationRecord, Guid>
    {
        Task<PagedList<VaccinationRecord>> GetRecordsAsync(
            Guid? studentId,
            Guid? scheduleId,
            Guid? vaccineTypeId,
            DateTime? from,
            DateTime? to,
            int pageNumber,
            int pageSize,
            string? searchTerm = null);

        Task<PagedList<VaccinationRecord>> GetRecordsByStudentAsync(Guid studentId, int pageNumber, int pageSize, string? searchTerm = null);
        Task<PagedList<VaccinationRecord>> GetRecordsByScheduleAsync(Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null);
        Task<List<VaccinationRecord>> GetRecordsByDateAsync(DateTime from, DateTime to, string? searchTerm = null);
        Task<VaccinationRecord?> GetRecordWithDetailsAsync(Guid id);
        Task<List<VaccinationRecord>> GetRecordsByIdsAsync(List<Guid> ids, bool includeDeleted = false);
        Task<bool> UpdateReactionFollowUpAsync(Guid id, bool followup24h, bool followup72h);
        Task<bool> BatchUpdateReactionSeverityAsync(List<Guid> ids, VaccinationReactionSeverity severity);
        Task<PagedList<VaccinationRecord>> GetSoftDeletedRecordsAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> RestoreRecordAsync(Guid id, Guid restoredBy);
        Task<bool> BatchRestoreRecordsAsync(List<Guid> ids, Guid restoredBy);
        Task<bool> HasDuplicateEntryAsync(Guid studentId, Guid scheduleId);
        Task<List<VaccinationRecord>> GetRecordsByStudentAsync(Guid studentId);
    }
}
