using DTOs.VaccinationRecordDTOs.Request;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class VaccinationRecordController : ControllerBase
    {
        private readonly IVaccinationRecordService _vaccinationRecordService;

        public VaccinationRecordController(IVaccinationRecordService vaccinationRecordService)
        {
            _vaccinationRecordService = vaccinationRecordService;
        }

        /// <summary>
        /// Tạo phiếu tiêm chủng mới
        /// </summary>
        [HttpPost]
        // [Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> Create([FromBody] CreateVaccinationRecordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _vaccinationRecordService.CreateAsync(request);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Cập nhật thông tin phiếu tiêm
        /// </summary>
        [HttpPut("{id}")]
        // [Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaccinationRecordRequest request)
        {
            var result = await _vaccinationRecordService.UpdateAsync(id, request);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Xóa mềm phiếu tiêm
        /// </summary>
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid deletedBy)
        {
            var result = await _vaccinationRecordService.DeleteAsync(id, deletedBy);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Lấy chi tiết phiếu tiêm theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _vaccinationRecordService.GetByIdAsync(id);
            if (result.IsSuccess)
                return Ok(result);

            return NotFound(result);
        }

        /// <summary>
        /// Lấy danh sách phiếu tiêm theo lịch tiêm
        /// </summary>
        [HttpGet("schedule/{scheduleId}")]
        public async Task<IActionResult> GetBySchedule(
            Guid scheduleId,
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _vaccinationRecordService.GetRecordsByScheduleAsync(scheduleId, pageNumber, pageSize, searchTerm);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách phiếu tiêm theo học sinh
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(
            Guid studentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _vaccinationRecordService.GetRecordsByStudentAsync(studentId, pageNumber, pageSize, searchTerm);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}