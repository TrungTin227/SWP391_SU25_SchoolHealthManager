using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class CounselingAppointmentRepository : GenericRepository<CounselingAppointment, Guid>, ICounselingAppointmentRepository
    {
        public CounselingAppointmentRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
    }
}
