﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class NurseProfileRepository : GenericRepository<NurseProfile, Guid>, INurseProfileRepository
    {
        public NurseProfileRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }

        public async Task<NurseProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _context.NurseProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        }

    }
}
