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

        [HttpPost("acpt-Appointment")]
        public async Task<IActionResult> AcptAppointment(Guid AppointmentId)
        {
            if (AppointmentId == Guid.Empty)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.AcceptAppointmentAsync(AppointmentId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("reject-Appointment")]
        public async Task<IActionResult> RejectAppointment(Guid AppointmentId)
        {
            if (AppointmentId == Guid.Empty)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.RejectAppointmentAsync(AppointmentId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("update-Appointment-by-id")]
        public async Task<IActionResult> UpdateAppointmentById([FromBody] UpdateCounselingAppointmentRequestDTO request)
        {
            if (request == null || request.Id == Guid.Empty)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.UpdateAppointmentAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("get-appointments-by-id")]
        public async Task<IActionResult> GetById(Guid AppointmentId)
        {
            if (AppointmentId == Guid.Empty)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.GetByIdAsync(AppointmentId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get-pending-appointments-by-staff-id")]
        public async Task<IActionResult> GetPendingAppointmentsByStaffId(Guid StaffId)
        {
            if (StaffId == Guid.Empty)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.GetAllPendingByStaffIdAsync(StaffId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("get-all-appointments-by-staff-id")]
        public async Task<IActionResult> GetAllAppointmentsByStaffId(Guid StaffId)
        {
            if (StaffId == Guid.Empty)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.GetAllByStaffIdAsync(StaffId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get-all-appointments-by-student-code")]
        public async Task<IActionResult> GetAllAppointmentsByStudentCode(string StudentCode)
        {
            if (string.IsNullOrEmpty(StudentCode))
            {
                return BadRequest(new { Message = "Yêu cầu nhập mã học sinh hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.GetAllByStudentCodeAsync(StudentCode);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("delete-appointment-by-id")]
        public async Task<IActionResult> DeleteAppointmentById(List<Guid> AppointmentId)
        {
            if (AppointmentId == null || !AppointmentId.Any())
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });
            }
            var result = await _counselingAppointmentService.SoftDeleteRangeAsync(AppointmentId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
