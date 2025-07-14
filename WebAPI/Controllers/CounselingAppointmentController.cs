using DTOs.CounselingAppointmentDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/counseling-appointments")]
    public class CounselingAppointmentController : Controller
    {
        private readonly ICounselingAppointmentService _counselingAppointmentService;

        public CounselingAppointmentController(ICounselingAppointmentService counselingAppointmentService)
        {
            _counselingAppointmentService = counselingAppointmentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCounselingAppointment([FromBody] CreateCounselingAppointmentRequestDTO request)
        {
            if (request == null)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });

            var result = await _counselingAppointmentService.CreateCounselingAppointmentAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("add-note")]
        public async Task<IActionResult> AddNoteAndRecommend([FromBody] AddNoteAndRecommendRequestDTO request)
        {
            if (request == null)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });

            var result = await _counselingAppointmentService.AddNoteAndRecommend(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{appointmentId}/accept")]
        public async Task<IActionResult> AcptAppointment(Guid appointmentId)
        {
            if (appointmentId == Guid.Empty)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _counselingAppointmentService.AcceptAppointmentAsync(appointmentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{appointmentId}/reject")]
        public async Task<IActionResult> RejectAppointment(Guid appointmentId)
        {
            if (appointmentId == Guid.Empty)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _counselingAppointmentService.RejectAppointmentAsync(appointmentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointmentById([FromBody] UpdateCounselingAppointmentRequestDTO request)
        {
            if (request == null || request.Id == Guid.Empty)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _counselingAppointmentService.UpdateAppointmentAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetById(Guid appointmentId)
        {
            if (appointmentId == Guid.Empty)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _counselingAppointmentService.GetByIdAsync(appointmentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("pending/staff/{staffId}")]
        public async Task<IActionResult> GetPendingAppointmentsByStaffId(Guid staffId)
        {
            if (staffId == Guid.Empty)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _counselingAppointmentService.GetAllPendingByStaffIdAsync(staffId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetAllAppointmentsByStaffId(Guid staffId)
        {
            if (staffId == Guid.Empty)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _counselingAppointmentService.GetAllByStaffIdAsync(staffId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("code/{studentCode}")]
        public async Task<IActionResult> GetAllAppointmentsByStudentCode(string studentCode)
        {
            if (string.IsNullOrEmpty(studentCode))
                return BadRequest(new { Message = "Yêu cầu nhập mã học sinh hợp lệ!!" });

            var result = await _counselingAppointmentService.GetAllByStudentCodeAsync(studentCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("id/{studentId}")]
        public async Task<IActionResult> GetAllAppointmentsByStudentId(Guid studentId)
        {
            //if (string.IsNullOrEmpty(studentCode))
            //    return BadRequest(new { Message = "Yêu cầu nhập mã học sinh hợp lệ!!" });

            var result = await _counselingAppointmentService.GetAllByStudentIdAsync(studentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAppointmentById([FromBody] List<Guid> appointmentIds)
        {
            if (appointmentIds == null || !appointmentIds.Any())
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu hợp lệ!!" });

            var result = await _counselingAppointmentService.SoftDeleteRangeAsync(appointmentIds);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("restore-counseling-appointments")]
        public async Task<IActionResult> RestoreCounselingAppointments([FromBody] List<Guid> ids)
        {
            var result = await _counselingAppointmentService.RestoreCounselingAppointmentRangeAsync(ids, null);
            return Ok(result);
        }
    }
}
