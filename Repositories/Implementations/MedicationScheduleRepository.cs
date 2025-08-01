using BusinessObjects;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class MedicationScheduleRepository : GenericRepository<MedicationSchedule, Guid>, IMedicationScheduleRepository
    {
        public MedicationScheduleRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
    }
} 