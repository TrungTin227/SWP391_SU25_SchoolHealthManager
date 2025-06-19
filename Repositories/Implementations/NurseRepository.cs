using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using DTOs.NurseDTOs.Request;
using DTOs.NurseDTOs.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementations
{
    public class NurseRepository : GenericRepository<NurseProfile,Guid>, INurseRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SchoolHealthManagerDbContext _dbcontext;
        public NurseRepository(UserManager<User> userManager, SchoolHealthManagerDbContext dbcontext) : base(dbcontext)
        {
            _userManager = userManager;
            _dbcontext = dbcontext;
        }

        public async Task AddAsync(User user)
        {
            await _userManager.CreateAsync(user);
        }

        public async Task<NurseProfile> CreateNurseAsync(NurseProfile nurse)
        {
            await _dbcontext.NurseProfiles.AddAsync(nurse);
            _dbcontext.SaveChanges();
            return nurse;
        }

        public async Task<bool> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;
            return true;
        }

        public Task<List<NurseProfile>> GetNurseAsync()
        {
            return _dbcontext.NurseProfiles
                .Where(s => !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<NurseProfile?> GetNurseByUserIdAsync(Guid userId)
        {
            return await _dbcontext.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public Task<List<GetNurseDTO>> GetNurseDtoAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SoftDeleteByNurseId(Guid nurseId)
        {
            var nurse = await _dbcontext.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == nurseId);
            if (nurse == null)
            {
                return false;
            }
            nurse.IsDeleted = true;
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public Task<bool> UpdateNurseAsync(UpdateNurseRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
