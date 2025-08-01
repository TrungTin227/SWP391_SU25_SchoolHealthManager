using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class MedicationUsageRecordRepository : GenericRepository<MedicationUsageRecord, Guid>, IMedicationUsageRecordRepository
    {
        public MedicationUsageRecordRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
    }
}
