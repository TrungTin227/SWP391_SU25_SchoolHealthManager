using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        public async Task<string> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;
            return user.Email;
        }

        public Task<Parent?> GetParentByUserIdAsync(Guid userId)
        =>  _dbcontext.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
        
    }
}
