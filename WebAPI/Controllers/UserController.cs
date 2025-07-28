using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "ADMIN")]  // <-- mọi action ở đây đều yêu cầu role=ADMIN
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var response = await _userService.GetByIdAsync(id);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            var response = await _userService.UpdateAsync(id, request);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteUser([FromQuery] List<Guid> ids)
        {
            var result = await _userService.DeleteUsersAsync(ids);
            if (!result.IsSuccess)
                return BadRequest(result);   // Trả về 400 cùng message "User {id} not found"
            return NoContent();
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchTerm = null,
    [FromQuery] RoleType? role = null)
        {
            var result = await _userService.SearchUsersAsync(searchTerm, role, pageNumber, pageSize);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        // Endpoint lock user
        [HttpPut("lock")]
        public async Task<IActionResult> LockUser([FromQuery] Guid id)
        {
            var response = await _userService.LockUserAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // Endpoint unlock user
        [HttpPut("unlock")]
        public async Task<IActionResult> UnlockUser([FromQuery] Guid id)
        {
            var response = await _userService.UnlockUserAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //[HttpPost("admin/register")]
        //public async Task<IActionResult> RegisterAdmin([FromBody] AdminCreateUserRequest request)
        //{
        //    var result = await _userService.AdminRegisterAsync(request);
        //    if (!result.IsSuccess)
        //        return BadRequest(result);

        //    return CreatedAtAction(nameof(GetUserById),
        //                           new { id = result.Data!.Id },
        //                           result);
        //}
    }
}
