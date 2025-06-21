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
    }
}
