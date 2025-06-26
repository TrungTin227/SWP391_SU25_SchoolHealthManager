using DTOs.CounselingAppointmentDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CounselingAppointmentController : Controller
    {
        public readonly ICounselingAppointmentService _counselingAppointmentService;
        public CounselingAppointmentController(ICounselingAppointmentService counselingAppointmentService)
        {
            _counselingAppointmentService = counselingAppointmentService;
        }
        [HttpPost("create-counseling-appointment")]
        public async Task<IActionResult> CreateCounselingAppointment([FromBody] CreateCounselingAppointmentRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });
            }
            var result = await _counselingAppointmentService.CreateCounselingAppointmentAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("add-note-and-recommend")]
        public async Task<IActionResult> AddNoteAndRecommend(AddNoteAndRecommendRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });
            }

            var result = await _counselingAppointmentService.AddNoteAndRecommend(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
