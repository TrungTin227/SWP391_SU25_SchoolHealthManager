using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class ParentController : Controller
    {
        private readonly IParentService _parentService;

        public ParentController(IParentService parentService) 
        { 
            _parentService = parentService; 
        }

        [HttpPost("parent/registerUser")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "Email and Password are required" });
            }

            var result = await _parentService.RegisterUserAsync(request);

            if (result == null) // Hoặc if (!result.Success) tùy vào kiểu trả về
            {
                return BadRequest(new { Message = "User registration failed" });
            }

            return Ok(new { Message = "User registered successfully" });
        }

    }
}
