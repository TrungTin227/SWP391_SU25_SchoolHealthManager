using DTOs.HealProfile.Requests;
using Microsoft.AspNetCore.Mvc;
using Quartz.Util;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthProfileController : Controller
    {
       public readonly IHealProfileService _healProfileService;
        public HealthProfileController(IHealProfileService healProfileService)
        {
            _healProfileService = healProfileService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateHealthProfile([FromBody] CreateHealProfileRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }
            var result = await _healProfileService.CreateHealProfileAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpPost("update-healprofile-by-studentcode/{studentCode}")]
        public async Task<IActionResult> UpdateHealthProfileById(string studentCode, [FromBody] UpdateHealProfileRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }
            if (studentCode.IsNullOrWhiteSpace())
            {
                return BadRequest("Mã học sinh không được null!!");
            }
            var result = await _healProfileService.UpdateHealProfileByStudentCodeAsync(studentCode, request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete]
        [Route("soft-delete-healprofile-by-id/{id}")]
        public async Task<IActionResult> DeleteHealthProfileById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Id của hồ sơ sức khỏe không được null!!");
            }
            var result = await _healProfileService.SoftDeleteHealProfileAsync(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("get-all-healprofiles-by-student-code/{code}")]
        public async Task<IActionResult> GetAllHealProfilesByStudentId(string code)
        {
            if (code.IsNullOrWhiteSpace())
            {
                return BadRequest("Mã học sinh không được null!!");
            }
            var result = await _healProfileService.GetAllHealProfileByStudentCodeAsync(code);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("get-healprofile-by-id/{id}")]
        public async Task<IActionResult> GetHealProfileById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Id của hồ sơ sức khỏe không được null!!");
            }
            var result = await _healProfileService.GetHealProfileByIdAsync(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("get-newest-healprofile-by-student-code/{studentcode}")]
        public async Task<IActionResult> GetHealProfileByStudentId(string studentcode)
        {
            if (studentcode.IsNullOrWhiteSpace())
            {
                return BadRequest("Mã học sinh không được null!!");
            }
            var result = await _healProfileService.GetNewestHealProfileByStudentCodeAsync(studentcode);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        //[HttpGet("get-newest-healprofile-by-parent-id/{parentId}")]
        //public async Task<IActionResult> GetHealProfileByParentId(Guid parentId)
        //{
        //    if (parentId == Guid.Empty)
        //    {
        //        return BadRequest("Id của phụ huynh không được null!!");
        //    }
        //    var result = await _healProfileService.GetHealProfileByParentIdAsync(parentId);
        //    if (result.IsSuccess)
        //    {
        //        return Ok(result);
        //    }
        //    return BadRequest(result);
        //}
    }
}
