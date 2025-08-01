﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISessionStudentRepository : IGenericRepository<SessionStudent, Guid>
    {
        Task<SessionStudent?> GetByStudentAndScheduleAsync(Guid studentId, Guid scheduleId);

    }
}
