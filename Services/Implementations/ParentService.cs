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
        private readonly IUserService _userService;
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public ParentService(IParentRepository parentRepository, UserManager<User> userManager, ICurrentUserService currentUserService, IUserService userService)
        {
            _parentRepository = parentRepository;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _userService = userService;
        }

        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user)
        {
            var existing = await _parentRepository.FindByEmailAsync(user.Email);
            if (existing != null)
            {
                return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Mail đã được sử dụng, vui lòng sử dụng mail khác!!"));
            }
            var currentUserId = _currentUserService.GetUserId()?? SystemGuid;

            //if (currentUserId == SystemGuid)
            //{
            //    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Current user is not authenticated."));
            //}

            var newUser = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.Email,
                Gender = user.Gender,
                CreatedAt = DateTime.UtcNow,
                //CreatedBy = currentUserId,
                UpdatedAt = DateTime.UtcNow,
                //UpdatedBy = currentUserId,
                IsDeleted = false,
                EmailConfirmed = false  // optional
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return ApiResult<UserRegisterRespondDTO>.Failure(new Exception($"Register failed: {errors}"));
            }
            else if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "Parent");
                _userService.SendWelcomeEmailsAsync(newUser.Email);
            }

            return ApiResult<UserRegisterRespondDTO>.Success(UserMappings.ToUserRegisterResponse(newUser), "User registered successfully.");
        }
    }
}
