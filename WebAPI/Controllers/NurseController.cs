using DTOs.NurseDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NurseController : Controller
    {
        private readonly INurseService _nurseService;


        public NurseController(INurseService nurseService)
        {
            _nurseService = nurseService;
        }
        [HttpPost("register-nurse")]
        public async Task<IActionResult> RegisterParent([FromBody] UserRegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _nurseService.RegisterNurseUserAsync(request);
            if (result.IsSuccess)
                return Ok(result);
            else
                return BadRequest(result);
        }
        //[HttpPost("register-User")]
        //public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequestDTO request)
        //{
        //    if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        //    {
        //        return BadRequest(new { Message = "Email and Password are required" });
        //    }

        //    var result = await _nurseService.RegisterUserAsync(request);

        //    if (!result.IsSuccess)
        //    {
        //        return BadRequest(result);
        //    }

        //    return Ok(result);
        //}

        //[HttpPost("create-nurse")]
        //public async Task<IActionResult> CreateParent([FromBody] AddNurseRequestDTO request)
        //{
        //    if (request == null)
        //    {
        //        return BadRequest(new { Message = "User ID are required" });
        //    }
        //    var result = await _nurseService.CreateNurseAsync(request);
        //    if (!result.IsSuccess)
        //    {
        //        return BadRequest(result);
        //    }
        //    return Ok(result);
        //}

        //[HttpGet("get-all-nurses")]
        //public async Task<IActionResult> GetAllParents()
        //{
        //    var result = await _nurseService.GetAllNursesAsync();
        //    if (!result.IsSuccess)
        //    {
        //        return BadRequest(result);
        //    }
        //    return Ok(result);
        //}

        [HttpDelete("soft-delete-by-nurse-id")]
        public async Task<IActionResult> SoftDeleteByParentId([FromBody] Guid parentId)
        {
            if (parentId == Guid.Empty)
            {
                return BadRequest(new { Message = "Parent ID is required" });
            }
            var result = await _nurseService.SoftDeleteByNurseIdAsync(parentId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}
