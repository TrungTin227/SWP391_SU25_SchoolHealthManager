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
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });
            }
            var result = await _sessionStudentService.ParentAcptVaccineAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
