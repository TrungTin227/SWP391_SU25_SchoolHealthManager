using DTOs.HealProfile.Requests;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("update-healprofile-by-id/{id}")]
        public async Task<IActionResult> UpdateHealthProfileById(Guid id, [FromBody] UpdateHealProfileRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }
            if (id == Guid.Empty)
            {
                return BadRequest("Id của hồ sơ sức khỏe không được null!!");
            }
            var result = await _healProfileService.UpdateHealProfileByIdAsync(id, request);
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

        [HttpGet("get-all-healprofiles-by-student-id/{id}")]
        public async Task<IActionResult> GetAllHealProfilesByStudentId(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Id của học sinh không được null!!");
            }
            var result = await _healProfileService.GetAllHealProfileByStudentIdAsync(id);
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

        [HttpGet("get-newest-healprofile-by-student-id/{studentId}")]
        public async Task<IActionResult> GetHealProfileByStudentId(Guid studentId)
        {
            if (studentId == Guid.Empty)
            {
                return BadRequest("Id của học sinh không được null!!");
            }
            var result = await _healProfileService.GetHealProfileByStudentIdAsync(studentId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("get-newest-healprofile-by-parent-id/{parentId}")]
        public async Task<IActionResult> GetHealProfileByParentId(Guid parentId)
        {
            if (parentId == Guid.Empty)
            {
                return BadRequest("Id của phụ huynh không được null!!");
            }
            var result = await _healProfileService.GetHealProfileByParentIdAsync(parentId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
