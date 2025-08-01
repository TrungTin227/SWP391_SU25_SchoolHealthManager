﻿using Microsoft.EntityFrameworkCore;


namespace Repositories.Implementations
{
    public class HealthEventRepository : GenericRepository<HealthEvent, Guid>, IHealthEventRepository
    {
        public HealthEventRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }

        public async Task<PagedList<HealthEvent>> GetHealthEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            EventStatus? status = null, EventType? eventType = null,
            Guid? studentId = null, DateTime? fromDate = null, DateTime? toDate = null, Guid? parentUserId = null)
        {
            var query = _context.HealthEvents
                .Include(he => he.Student)
                .Include(he => he.ReportedUser)
                    .ThenInclude(u => u.StaffProfile)
                .Include(he => he.EventMedications)
                .Include(he => he.SupplyUsages)
                .Where(he => !he.IsDeleted);
            // ✅ Filter theo parentUserId (cho phụ huynh chỉ xem sự kiện của con mình)
            if (parentUserId.HasValue)
            {
                query = query.Where(he => he.Student.ParentUserId == parentUserId.Value);
            }
            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(he =>
                        EF.Functions.Like(he.Description.ToLower(), $"%{term}%") ||
                        EF.Functions.Like(
                            (he.Student.FirstName ?? "") + " " + (he.Student.LastName ?? ""),
                            $"%{term}%") ||
                        EF.Functions.Like(
                            (he.ReportedUser.FirstName ?? "") + " " + (he.ReportedUser.LastName ?? ""),
                            $"%{term}%")
                    );
            }

            if (status.HasValue)
                query = query.Where(he => he.EventStatus == status.Value);

            if (eventType.HasValue)
                query = query.Where(he => he.EventType == eventType.Value);

            if (studentId.HasValue)
                query = query.Where(he => he.StudentId == studentId.Value);

            if (fromDate.HasValue)
                query = query.Where(he => he.OccurredAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(he => he.OccurredAt <= toDate.Value);

            query = query.OrderByDescending(he => he.CreatedAt);

            return await PagedList<HealthEvent>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<HealthEvent?> GetHealthEventWithDetailsAsync(Guid id)
        {
            return await _context.HealthEvents
                //.AsNoTracking()
                .Include(he => he.Student)
                    .ThenInclude(s => s.Parent)
                        .ThenInclude(p => p.User)
                .Include(he => he.ReportedUser)
                .Include(he => he.FirstResponder)
                            .ThenInclude(fr => fr.User) 
                .Include(he => he.EventMedications)
                    .ThenInclude(em => em.MedicationLot)
                        .ThenInclude(ml => ml.Medication)
                .Include(he => he.SupplyUsages)
                    .ThenInclude(su => su.MedicalSupplyLot)
                        .ThenInclude(msl => msl.MedicalSupply)
                .Include(he => he.SupplyUsages)
                    .ThenInclude(su => su.UsedByNurse)
                .FirstOrDefaultAsync(he => he.Id == id && !he.IsDeleted);
        }
        public async Task<List<HealthEvent>> GetHealthEventsByIdsAsync(List<Guid> ids, bool includeDeleted = false)
        {
            var query = _context.HealthEvents.AsQueryable();

            if (!includeDeleted)
                query = query.Where(he => !he.IsDeleted);

            return await query.Where(he => ids.Contains(he.Id)).ToListAsync();
        }

        public async Task<int> SoftDeleteHealthEventsAsync(List<Guid> ids, Guid deletedBy)
        {
            var events = await _context.HealthEvents
                .Where(he => ids.Contains(he.Id) && !he.IsDeleted)
                .ToListAsync();

            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            foreach (var healthEvent in events)
            {
                healthEvent.IsDeleted = true;
                healthEvent.UpdatedBy = deletedBy;
                healthEvent.UpdatedAt = vietnamTime;
            }

            return events.Count;
        }

        public async Task<int> RestoreHealthEventsAsync(List<Guid> ids, Guid restoredBy)
        {
            var events = await _context.HealthEvents
                .Where(he => ids.Contains(he.Id) && he.IsDeleted)
                .ToListAsync();

            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            foreach (var healthEvent in events)
            {
                healthEvent.IsDeleted = false;
                healthEvent.UpdatedBy = restoredBy;
                healthEvent.UpdatedAt = vietnamTime;
            }

            return events.Count;
        }

        public async Task<int> PermanentDeleteHealthEventsAsync(List<Guid> ids)
        {
            // Lấy các sự kiện cần xóa CÙNG VỚI các bản ghi con của chúng
            var eventsToDelete = await _context.HealthEvents
                .Include(he => he.SupplyUsages)       // Tải các SupplyUsage liên quan
                .Include(he => he.EventMedications) // Tải các EventMedication liên quan
                .Where(he => ids.Contains(he.Id))
                .ToListAsync();

            if (!eventsToDelete.Any())
            {
                return 0;
            }         
            _context.HealthEvents.RemoveRange(eventsToDelete);

            return await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateEventStatusAsync(Guid eventId, EventStatus newStatus, Guid updatedBy)
        {
            var healthEvent = await _context.HealthEvents
                .FirstOrDefaultAsync(he => he.Id == eventId && !he.IsDeleted);

            if (healthEvent == null) return false;

            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            healthEvent.EventStatus = newStatus;
            healthEvent.UpdatedBy = updatedBy;
            healthEvent.UpdatedAt = vietnamTime;

            return true;
        }

        public async Task<PagedList<HealthEvent>> GetSoftDeletedEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.HealthEvents
                .Include(he => he.Student)
                .Include(he => he.ReportedUser)
                .Where(he => he.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(he =>
                    he.Description.ToLower().Contains(term) ||
                    he.Student.FullName.ToLower().Contains(term));
            }

            query = query.OrderByDescending(he => he.UpdatedAt);

            return await PagedList<HealthEvent>.ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<Dictionary<EventStatus, int>> GetEventStatusStatisticsAsync(
    DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Bắt đầu với truy vấn gọn nhất có thể
            var query = _context.HealthEvents.AsNoTracking().Where(he => !he.IsDeleted);

            if (fromDate.HasValue) query = query.Where(he => he.OccurredAt >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(he => he.OccurredAt <= toDate.Value);

            return await query
                .GroupBy(he => he.EventStatus)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<EventType, int>> GetEventTypeStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.HealthEvents.Where(he => !he.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(he => he.OccurredAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(he => he.OccurredAt <= toDate.Value);

            return await query
                .GroupBy(he => he.EventType)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        // File: Repositories/Implementations/HealthEventRepository.cs

        public async Task<List<HealthEvent>> GetHealthEventsByStudentIdsAsync(HashSet<Guid> studentIds)
        {
            return await _context.HealthEvents
                .Where(e => studentIds.Contains(e.StudentId) && !e.IsDeleted)

                // --- BỔ SUNG ĐẦY ĐỦ CÁC INCLUDE DƯỚI ĐÂY ---

                // Tải thông tin cơ bản
                .Include(he => he.Student)
                .Include(he => he.ReportedUser)

                // Tải thông tin người sơ cứu (Y tá)
                .Include(he => he.FirstResponder)
                    .ThenInclude(fr => fr.User) // Quan trọng: Phải ThenInclude để lấy được User từ NurseProfile

                // Tải danh sách thuốc đã sử dụng và thông tin chi tiết của thuốc
                .Include(he => he.EventMedications)
                    .ThenInclude(em => em.MedicationLot)
                        .ThenInclude(ml => ml.Medication)

                // Tải danh sách vật tư đã sử dụng và thông tin chi tiết của vật tư
                .Include(he => he.SupplyUsages)
                    .ThenInclude(su => su.MedicalSupplyLot)
                        .ThenInclude(msl => msl.MedicalSupply)

                // Tải thông tin y tá đã sử dụng vật tư
                .Include(he => he.SupplyUsages)
                    .ThenInclude(su => su.UsedByNurse)
                        .ThenInclude(un => un.User) // Quan trọng: Lấy User từ NurseProfile của người dùng vật tư

                // ---------------------------------------------------

                .OrderByDescending(e => e.OccurredAt)// Sắp xếp theo ngày tạo mới nhất để phụ huynh dễ theo dõi
                .ToListAsync();
        }
    }
}
