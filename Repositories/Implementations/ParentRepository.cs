using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Repositories.Implementations
{
    public class ParentRepository : IParentRepository
    {
        private readonly UserManager<User> _userManager;

        public ParentRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task AddAsync(User user)
        {
            await _userManager.CreateAsync(user);
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
    }
}
