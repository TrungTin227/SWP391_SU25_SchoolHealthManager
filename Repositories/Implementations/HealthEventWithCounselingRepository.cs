using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.HealthEventDTOs.Request;
using DTOs.HealthEventDTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementations
{
    public class HealthEventWithCounselingRepository : IHealthEventWithCounselingRepository
    {
        private readonly SchoolHealthManagerDbContext _context;
        public async Task<HealthEventWithCounselingResponse?> CreateWithCounselingAsync(HealthEventCreateWithCounselingRequest request)
        {

            var student = await _context.Students
     .Include(s => s.Parent)
     .FirstOrDefaultAsync(s => s.Id == request.StudentId);

            if (student == null) throw new Exception("Student not found");
            if (student.Parent == null) throw new Exception("Parent not found");

            var parent = student.Parent;

            var healthEvent = new HealthEvent
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                VaccinationRecordId = request.VaccinationRecordId,
                EventCategory = request.EventCategory,
                EventType = request.EventType,
                Description = request.Description,
                OccurredAt = request.OccurredAt,
                EventStatus = request.EventStatus,
                ReportedUserId = request.ReportedUserId
            };
            _context.HealthEvents.Add(healthEvent);

            CounselingAppointment? counseling = null;
            if (request.AppointmentDate.HasValue && request.StaffUserId.HasValue)
            {
                counseling = new CounselingAppointment
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    ParentId = parent.UserId,
                    StaffUserId = request.StaffUserId.Value,
                    AppointmentDate = request.AppointmentDate.Value,
                    Duration = request.Duration ?? 30,
                    Purpose = request.Purpose,
                    Status = ScheduleStatus.Scheduled,
                    VaccinationRecordId = request.VaccinationRecordId
                };
                _context.CounselingAppointments.Add(counseling);
            }

            await _context.SaveChangesAsync();

            return new HealthEventWithCounselingResponse
            {
                HealthEventId = healthEvent.Id,
                StudentId = student.Id,
                StudentName = student.FullName,
                VaccinationRecordId = healthEvent.VaccinationRecordId,
                EventCategory = healthEvent.EventCategory,
                EventType = healthEvent.EventType,
                Description = healthEvent.Description,
                OccurredAt = healthEvent.OccurredAt,
                EventStatus = healthEvent.EventStatus,
                ReportedUserId = healthEvent.ReportedUserId,
                ReportedUserName = (await _context.Users.FindAsync(healthEvent.ReportedUserId))?.FullName ?? "",

                CounselingAppointmentId = counseling?.Id,
                AppointmentDate = counseling?.AppointmentDate,
                Duration = counseling?.Duration,
                Purpose = counseling?.Purpose,
                StaffUserId = counseling?.StaffUserId,
                StaffUserName = counseling?.StaffUser?.User?.UserName,
            }; ;
        }

        public async Task<HealthEventWithCounselingResponse?> GetByIdWithCounselingAsync(Guid id)
        {
            var entity = await _context.HealthEvents
        .Include(e => e.Student)
        .Include(e => e.ReportedUser)
        .Include(e => e.VaccinationRecord)
        .Include(e => e.Student.Parent)
        .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null) return null;

            var counseling = await _context.CounselingAppointments
                .Include(c => c.StaffUser)
                .FirstOrDefaultAsync(c => c.VaccinationRecordId == entity.VaccinationRecordId);

            return new HealthEventWithCounselingResponse
            {
                HealthEventId = entity.Id,
                StudentId = entity.StudentId,
                StudentName = entity.Student?.FullName ?? "",
                VaccinationRecordId = entity.VaccinationRecordId,
                EventCategory = entity.EventCategory,
                EventType = entity.EventType,
                Description = entity.Description,
                OccurredAt = entity.OccurredAt,
                EventStatus = entity.EventStatus,
                ReportedUserId = entity.ReportedUserId,
                ReportedUserName = entity.ReportedUser?.FullName ?? "",
                CounselingAppointmentId = counseling?.Id,
                AppointmentDate = counseling?.AppointmentDate,
                Duration = counseling?.Duration,
                Purpose = counseling?.Purpose,
                StaffUserId = counseling?.StaffUserId,
                StaffUserName = counseling?.StaffUser?.User.FullName
            };
        }

        public async Task<List<HealthEventWithCounselingResponse>> GetByStudentIdAsync(Guid studentId)
        {
            var events = await _context.HealthEvents
        .Include(e => e.Student)
        .Include(e => e.ReportedUser)
        .Where(e => e.StudentId == studentId)
        .ToListAsync();

            return events.Select(e => new HealthEventWithCounselingResponse
            {
                HealthEventId = e.Id,
                StudentId = e.StudentId,
                StudentName = e.Student.FullName,
                VaccinationRecordId = e.VaccinationRecordId,
                EventCategory = e.EventCategory,
                EventType = e.EventType,
                Description = e.Description,
                OccurredAt = e.OccurredAt,
                EventStatus = e.EventStatus,
                ReportedUserId = e.ReportedUserId,
                ReportedUserName = e.ReportedUser?.FullName ?? ""
            }).ToList();
        }

        public async Task<List<HealthEventWithCounselingResponse>> GetByStudentIdWithCounselingAsync(Guid studentId)
        {
            var events = await _context.HealthEvents
        .Include(e => e.Student)
        .Include(e => e.ReportedUser)
        .Where(e => e.StudentId == studentId)
        .ToListAsync();

            var responses = new List<HealthEventWithCounselingResponse>();

            foreach (var e in events)
            {
                var counseling = await _context.CounselingAppointments
                    .Include(c => c.StaffUser)
                    .FirstOrDefaultAsync(c => c.VaccinationRecordId == e.VaccinationRecordId);

                responses.Add(new HealthEventWithCounselingResponse
                {
                    HealthEventId = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student?.FullName ?? "",
                    VaccinationRecordId = e.VaccinationRecordId,
                    EventCategory = e.EventCategory,
                    EventType = e.EventType,
                    Description = e.Description,
                    OccurredAt = e.OccurredAt,
                    EventStatus = e.EventStatus,
                    ReportedUserId = e.ReportedUserId,
                    ReportedUserName = e.ReportedUser?.FullName ?? "",
                    CounselingAppointmentId = counseling?.Id,
                    AppointmentDate = counseling?.AppointmentDate,
                    Duration = counseling?.Duration,
                    Purpose = counseling?.Purpose,
                    StaffUserId = counseling?.StaffUserId,
                    StaffUserName = counseling?.StaffUser?.User.FullName
                });
            }

            return responses;
        }
    }
}
