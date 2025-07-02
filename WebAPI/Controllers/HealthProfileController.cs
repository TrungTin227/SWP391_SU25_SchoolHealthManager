using DTOs.HealProfile.Requests;
using Microsoft.AspNetCore.Mvc;
using Quartz.Util;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/health-profiles")]
    public class HealthProfileController : Controller
    {
        private readonly IHealProfileService _healProfileService;

        public HealthProfileController(IHealProfileService healProfileService)
        {
            _healProfileService = healProfileService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateHealthProfile([FromBody] CreateHealProfileRequestDTO request)
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            var result = await _healProfileService.CreateHealProfileAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("student/{studentCode}")]
        public async Task<IActionResult> UpdateHealthProfileByStudentCode(string studentCode, [FromBody] UpdateHealProfileRequestDTO request)
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            if (studentCode.IsNullOrWhiteSpace())
                return BadRequest("Mã học sinh không được null!!");

            var result = await _healProfileService.UpdateHealProfileByStudentCodeAsync(studentCode, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHealthProfileById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Id của hồ sơ sức khỏe không được null!!");

            var result = await _healProfileService.SoftDeleteHealProfileAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("student/{code}")]
        public async Task<IActionResult> GetAllHealProfilesByStudentCode(string code)
        {
            if (code.IsNullOrWhiteSpace())
                return BadRequest("Mã học sinh không được null!!");

            var result = await _healProfileService.GetAllHealProfileByStudentCodeAsync(code);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHealProfileById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Id của hồ sơ sức khỏe không được null!!");

            var result = await _healProfileService.GetHealProfileByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("student/{studentCode}/latest")]
        public async Task<IActionResult> GetNewestHealProfileByStudentCode(string studentCode)
        {
            if (studentCode.IsNullOrWhiteSpace())
                return BadRequest("Mã học sinh không được null!!");

            var result = await _healProfileService.GetNewestHealProfileByStudentCodeAsync(studentCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //[HttpGet("parent/{parentId}/latest")]
        //public async Task<IActionResult> GetNewestHealProfileByParentId(Guid parentId)
        //{
        //    if (parentId == Guid.Empty)
        //        return BadRequest("Id của phụ huynh không được null!!");

        //    var result = await _healProfileService.GetHealProfileByParentIdAsync(parentId);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}
    }
}
