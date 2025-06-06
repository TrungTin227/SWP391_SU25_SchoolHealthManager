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

        public async Task<string> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null; // hoặc throw exception, tuỳ logic đại ca
            return user.Email;
        }
    }
}
