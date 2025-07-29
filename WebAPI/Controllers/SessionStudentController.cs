using DTOs.SessionStudentDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/session-students")]
    [Authorize]

    public class SessionStudentController : Controller
    {
        private readonly ISessionStudentService _sessionStudentService;

        public SessionStudentController(ISessionStudentService sessionStudentService)
        {
            _sessionStudentService = sessionStudentService;
        }

        [HttpPost("parent/accept-vaccine")]
        public async Task<IActionResult> ParentAcceptVaccine([FromBody] ParentAcptVaccine request)
        {
            if (request == null)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });

            var result = await _sessionStudentService.ParentAcptVaccineAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("check-in-time")]
        public async Task<IActionResult> UpdateCheckinTimeById([FromBody] UpdateSessionStudentCheckInRequest request)
        {
            var result = await _sessionStudentService.UpdateCheckinTimeById(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionStudentsByStudentOrParentId([FromQuery] GetSessionStudentsRequest request)
        {
            if (request == null)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });

            var result = await _sessionStudentService.GetSessionStudentsWithOptionalFilterAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("status")]
        public async Task<IActionResult> UpdateStudentStatusById([FromBody] UpdateSessionStatus request)
        {
            if (request == null || request.SessionStudentIds.Count <= 0)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _sessionStudentService.UpdateSessionStudentStatus(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("restore-session-students")]
        public async Task<IActionResult> RestoreSessionStudents([FromBody] List<Guid> ids)
        {
            var result = await _sessionStudentService.RestoreSessionStudentRangeAsync(ids, null);
            return Ok(result);
        }
    }
}
