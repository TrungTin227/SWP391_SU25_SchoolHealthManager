using BusinessObjects;
using Repositories.Interfaces;

namespace Repositories.Interfaces
{
    public interface IMedicationScheduleRepository : IGenericRepository<MedicationSchedule, Guid>
    {
    }
} 