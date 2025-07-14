using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class ParentVaccinationRecordRepository : GenericRepository<ParentVaccinationRecord, Guid>, IParentVaccinationRecordRepository
    {
        public ParentVaccinationRecordRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
    }
}
