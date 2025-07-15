using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class SessionStudentRepository : GenericRepository<SessionStudent, Guid>, ISessionStudentRepository
    {
        public SessionStudentRepository(SchoolHealthManagerDbContext context) : base(context)
        {

        }
        public async Task<SessionStudent?> GetByStudentAndScheduleAsync(Guid studentId, Guid scheduleId)
        {
            return await _context.SessionStudents
                .Include(ss => ss.Student)
                .Include(ss => ss.VaccinationSchedule)
                .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.VaccinationScheduleId == scheduleId);
        }
    }
}
