using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.WorkSeeds.Implements;

namespace Repositories.Implementations
{
    public class HealProfileRepository : GenericRepository<HealthProfile, Guid>, IHealProfileRepository
    {
        public HealProfileRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
    }
}
