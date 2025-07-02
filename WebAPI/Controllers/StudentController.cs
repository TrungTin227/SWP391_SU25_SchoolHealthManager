using DTOs.StudentDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterStudent([FromBody] AddStudentRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.StudentCode) || string.IsNullOrEmpty(request.FirstName))
                return BadRequest(new { Message = "StudentCode and FirstName are required" });

            var result = await _studentService.AddStudentAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var result = await _studentService.GetAllStudentsDTOAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-code")]
        public async Task<IActionResult> GetStudentsByStudentCode([FromQuery] string studentCode)
        {
            if (string.IsNullOrEmpty(studentCode))
                return BadRequest(new { Message = "StudentCode is required" });

            var result = await _studentService.GetStudentByStudentCodeAsync(studentCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-parent")]
        public async Task<IActionResult> GetStudentsByParentId([FromQuery] Guid parentId)
        {
            if (parentId == Guid.Empty)
                return BadRequest(new { Message = "ParentId is required" });

            var result = await _studentService.GetStudentsByParentIdAsync(parentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudentById([FromBody] UpdateStudentRequestDTO request)
        {
            if (request == null || request.Id == Guid.Empty)
                return BadRequest(new { Message = "Id is required" });

            var result = await _studentService.UpdateStudentById(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("by-ids")]
        public async Task<IActionResult> SoftDeleteStudentById([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { Message = "Id is required" });

            var result = await _studentService.SoftDeleteStudentByIdsAsync(ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("by-codes")]
        public async Task<IActionResult> SoftDeleteStudentByCode([FromBody] List<string> studentCodes)
        {
            if (studentCodes == null || !studentCodes.Any())
                return BadRequest(new { Message = "StudentCode is required" });

            var result = await _studentService.SoftDeleteStudentByCodesAsync(studentCodes);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
