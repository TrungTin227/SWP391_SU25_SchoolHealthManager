using DTOs.ParentDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ParentController : Controller
    {
        private readonly IParentService _parentService;

        public ParentController(IParentService parentService)
        {
            _parentService = parentService;
        }

        [HttpPost("register-User")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "Email and Password are required" });
            }

            var result = await _parentService.RegisterUserAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("create-parent")]
        public async Task<IActionResult> CreateParent([FromBody] AddParentRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "User ID are required" });
            }
            var result = await _parentService.CreateParentAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get-all-parents")]
        public async Task<IActionResult> GetAllParents()
        {
            var result = await _parentService.GetAllParentsAsync();
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("update-relationship-by-parent-id")]
        public async Task<IActionResult> UpdateRelationshipByParentId([FromBody] UpdateRelationshipByParentId request)
        {
            if (request == null || request.ParentId == Guid.Empty)
            {
                return BadRequest(new { Message = "Parent ID is required" });
            }
            var result = await _parentService.UpdateRelationshipByParentIdAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("soft-delete-by-parent-id")]
        public async Task<IActionResult> SoftDeleteByParentId([FromBody] Guid parentId)
        {
            if (parentId == Guid.Empty)
            {
                return BadRequest(new { Message = "Parent ID is required" });
            }
            var result = await _parentService.SoftDeleteByParentIdAsync(parentId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
