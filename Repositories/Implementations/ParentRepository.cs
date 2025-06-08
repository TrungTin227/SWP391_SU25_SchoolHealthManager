using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentDTOs.Request;
using DTOs.ParentDTOs.Response;
using DTOs.StudentDTOs.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.WorkSeeds.Implements;

namespace Repositories.Implementations
{
    public class ParentRepository : GenericRepository<Parent,Guid>, IParentRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SchoolHealthManagerDbContext _dbcontext;
        public ParentRepository(UserManager<User> userManager, SchoolHealthManagerDbContext dbcontext) : base(dbcontext)
        {
            _userManager = userManager;
            _dbcontext = dbcontext;
        }

        public async Task AddAsync(User user)
        {
            await _userManager.CreateAsync(user);
        }

        public async Task<Parent> CreateParentAsync(Parent parent)
        {
            await _dbcontext.Parents.AddAsync(parent);
            _dbcontext.SaveChanges();
            return parent;
        }


        public async Task<bool> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;
            return true;
        }

        public Task<List<Parent>> GetAllParentAsync()
        {
            return _dbcontext.Parents
                .Where(s => !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<GetAllParentDTO>> GetAllParentDtoAsync()
        {
            return await _dbcontext.Parents
                .Include(p => p.User) // nếu cần username
                .Where(p => !p.IsDeleted)
                .Select(p => new GetAllParentDTO
                {
                    UserId = p.UserId,
                    Username = p.User != null ? p.User.UserName : null,
                    Relationship = p.Relationship.ToString()
                })
                .ToListAsync();
        }


        public Task<Parent?> GetParentByUserIdAsync(Guid userId)
        =>  _dbcontext.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

        public async Task<bool> UpdateRelationshipByParentIdAsync(UpdateRelationshipByParentId request)
        {
            var parent = await _dbcontext.Parents.FirstOrDefaultAsync(p => p.UserId == request.ParentId);
            if (parent == null)
            {
                return false;
            }

            parent.Relationship = request.Relationship;
            await _dbcontext.SaveChangesAsync();
            return true;
        }

    }
}
