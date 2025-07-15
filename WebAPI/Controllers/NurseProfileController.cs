using DTOs.NurseProfile.Request;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NurseProfileController : ControllerBase
    {
        private readonly INurseProfileService _nurseProfileService;

        public NurseProfileController(INurseProfileService nurseProfileService)
        {
            _nurseProfileService = nurseProfileService;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ hồ sơ y tá
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNursesAsync()
        {
            var result = await _nurseProfileService.GetAllNursesAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }



        /// <summary>
        /// Đăng ký tài khoản y tá và tạo hồ sơ y tá
        /// </summary>
        [HttpPost("register-nurse")]
        public async Task<IActionResult> RegisterNurseAsync([FromBody] UserRegisterRequestDTO request)
        {
            var result = await _nurseProfileService.RegisterNurseUserAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Tạo hồ sơ y tá (sử dụng khi user đã tồn tại)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNurseAsync([FromBody] AddNurseRequest request)
        {
            var result = await _nurseProfileService.CreateNurseAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật hồ sơ y tá
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateNurseAsync([FromBody] UpdateNurseRequest request)
        {
            var result = await _nurseProfileService.UpdateNurseAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Xóa mềm hồ sơ y tá theo NurseId
        /// </summary>
        [HttpDelete("{nurseId}")]
        public async Task<IActionResult> SoftDeleteByNurseIdAsync(Guid nurseId)
        {
            var result = await _nurseProfileService.SoftDeleteByNurseIdAsync(nurseId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
