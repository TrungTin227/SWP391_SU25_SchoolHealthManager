using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.NurseDTOs.Request;
using DTOs.NurseDTOs.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Repositories.Implementations;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class NurseService : INurseService
    {
        private readonly INurseRepository _nurseRepository;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private readonly ILogger<NurseService> _logger;

        public NurseService(INurseRepository nurseRepository, UserManager<User> userManager, ICurrentUserService currentUserService, IUserService userService, ILogger<NurseService> logger)
        {
            _nurseRepository = nurseRepository;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _userService = userService;
            _logger = logger;
        }
        public Task<ApiResult<AddNurseRequestDTO>> CreateNurseAsync(AddNurseRequestDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResult<List<GetNurseDTO>>> GetAllNursesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResult<UserRegisterRespondDTO>> RegisterNurseUserAsync(UserRegisterRequestDTO user)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResult<bool>> SoftDeleteByNurseIdAsync(Guid NurseId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResult<bool>> UpdateNurseAsync(UpdateNurseRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
