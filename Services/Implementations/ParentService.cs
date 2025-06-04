using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class ParentService : IParentService
    {
        private readonly IParentRepository _parentRepository;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public ParentService(IParentRepository parentRepository, UserManager<User> userManager, ICurrentUserService currentUserService)
        {
            _parentRepository = parentRepository;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResult<User>> RegisterUserAsync(UserRegisterRequestDTO user)
        {
            var existing = await _parentRepository.FindByEmailAsync(user.Email);
            if (existing != null)
            {
                return ApiResult<User>.Failure(new Exception($"User with email {user.Email} already exists."));
            }

            var newUser = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.Email,
                Gender = user.Gender,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.UpdatedBy,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = user.UpdatedBy,
                IsDeleted = false,
                EmailConfirmed = false  // optional
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return ApiResult<User>.Failure(new Exception($"Register failed: {errors}"));
            }
            else if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "Parent");
            }

            return ApiResult<User>.Success(newUser, "User registered successfully.");
        }
    }
}
