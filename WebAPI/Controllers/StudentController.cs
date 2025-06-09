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
        public async Task<IActionResult> RegisterStudent([FromBody] AddStudentRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.StudentCode) || string.IsNullOrEmpty(request.FirstName))
            {
                return BadRequest(new { Message = "StudentCode and FullName are required" });
            }
            var result = await _studentService.AddStudentAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get-all-students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var result = await _studentService.GetAllStudentsDTOAsync();
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get-students-by-studentcode")]
        public async Task<IActionResult> GetStudentsByStudentCode([FromQuery] string studentCode)
        {
            if (string.IsNullOrEmpty(studentCode))
            {
                return BadRequest(new { Message = "StudentCode is required" });
            }
            var result = await _studentService.GetStudentByStudentCodeAsync(studentCode);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("update-student-by-id")]
        public async Task<IActionResult> UpdateStudentById([FromBody] UpdateStudentRequestDTO request)
        {
            if (request == null || request.Id == Guid.Empty)
            {
                return BadRequest(new { Message = "Id is required" });
            }
            var result = await _studentService.UpdateStudentById(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("soft-delete-student-by-id")]
        public async Task<IActionResult> SoftDeleteStudentById([FromQuery] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Message = "Id is required" });
            }
            var result = await _studentService.SoftDeleteStudentByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("soft-delete-student-by-code")]
        public async Task<IActionResult> SoftDeleteStudentByCode([FromQuery] string studentCode)
        {
            if (string.IsNullOrEmpty(studentCode))
            {
                return BadRequest(new { Message = "StudentCode is required" });
            }
            var result = await _studentService.SoftDeleteStudentByCodeAsync(studentCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("get-students-by-parent-id")]
        public async Task<IActionResult> GetStudentsByParentId([FromQuery] Guid parentId)
        {
            if (parentId == Guid.Empty)
            {
                return BadRequest(new { Message = "ParentId is required" });
            }
            var result = await _studentService.GetStudentsByParentIdAsync(parentId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
