using DTOs.SessionStudentDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionStudentController : Controller
    {
        public readonly ISessionStudentService _sessionStudentService;
        public SessionStudentController(ISessionStudentService sessionStudentService)
        {
            _sessionStudentService = sessionStudentService;
        }
        [HttpPost("Parent-Acpt-Vaccine")]
        public async Task<IActionResult> ParentAcceptVaccine([FromBody] ParentAcptVaccine request)
        {

            if (request == null)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });

            var result = await _sessionStudentService.ParentAcptVaccineAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("Update-Checkin-Time-By-Id")]
        public async Task<IActionResult> UpdateCheckinTimeById([FromBody] UpdateSessionStudentCheckInRequest request)
        {
            var result = await _sessionStudentService.UpdateCheckinTimeById(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("get-session-students-with-optional-filter")]
        public async Task<IActionResult> GetSessionStudentsByStudentOrParentId([FromQuery] GetSessionStudentsRequest request)
        {
            if (request == null)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });
            var result = await _sessionStudentService.GetSessionStudentsWithOptionalFilterAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("update-student-status-by-id")]
        public async Task<IActionResult> UpdateStudentStatusById([FromBody] UpdateSessionStatus request)
        {
            if (request == null || request.SessionStudentIds.Count <= 0)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            var result = await _sessionStudentService.UpdateSessionStudentStatus(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
