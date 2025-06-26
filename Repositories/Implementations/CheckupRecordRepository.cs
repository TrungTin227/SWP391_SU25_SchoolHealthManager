using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class CheckupRecordRepository : GenericRepository<CheckupRecord, Guid>, ICheckupRecordRepository
    {
        public CheckupRecordRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
    }
}
