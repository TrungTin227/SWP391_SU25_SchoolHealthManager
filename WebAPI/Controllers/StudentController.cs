using DTOs.StudentDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService) 
        { 
            _studentService = studentService;
        }

        [HttpPost("register-Student")]
        public async Task<IActionResult> RegisterStudent([FromBody] AddStudentRequestDTO request) { 
            if (request == null || string.IsNullOrEmpty(request.StudentCode) || string.IsNullOrEmpty(request.FirstName))
            {
                return BadRequest(new { Message = "StudentCode and FullName are required" });
            }
            var result = await _studentService.AddStudentAsync(request);
            if (!result.IsSuccess) // Hoặc if (!result.Success) tùy vào kiểu trả về
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}
