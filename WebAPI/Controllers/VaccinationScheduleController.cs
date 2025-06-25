using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class VaccinationScheduleController : ControllerBase
    {
        private readonly IVaccinationScheduleService _scheduleService;

        public VaccinationScheduleController(IVaccinationScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        #region CRUD Operations

        /// <summary>
        /// Lấy danh sách lịch tiêm chủng với phân trang, tìm kiếm và bộ lọc linh hoạt
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] Guid? campaignId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] ScheduleStatus? status = null,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _scheduleService.GetSchedulesAsync(
                campaignId,
                startDate,
                endDate,
                status,
                searchTerm,
                pageNumber,
                pageSize
            );

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        /// <summary>
        /// Tạo lịch tiêm mới (có thể tạo hàng loạt theo khối/lớp)
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> CreateSchedules([FromBody] CreateVaccinationScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _scheduleService.CreateSchedulesAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpdateVaccinationScheduleRequest request)
        {
            var result = await _scheduleService.UpdateScheduleAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(Guid id)
        {
            var result = await _scheduleService.GetScheduleByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        #endregion

        #region Student Management

        [HttpPost("{scheduleId}/students")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> AddStudentsToSchedule(Guid scheduleId, [FromBody] List<Guid> studentIds)
        {
            var result = await _scheduleService.AddStudentsToScheduleAsync(scheduleId, studentIds);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{scheduleId}/students")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> RemoveStudentsFromSchedule(Guid scheduleId, [FromBody] List<Guid> studentIds)
        {
            var result = await _scheduleService.RemoveStudentsFromScheduleAsync(scheduleId, studentIds);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Status Management

        [HttpPatch("batch/status")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> BatchUpdateScheduleStatus([FromBody] BatchUpdateScheduleStatusRequest request)
        {
            var result = await _scheduleService.BatchUpdateScheduleStatusAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{scheduleId}/start")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> StartSchedule(Guid scheduleId)
        {
            var result = await _scheduleService.StartScheduleAsync(scheduleId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{scheduleId}/complete")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> CompleteSchedule(Guid scheduleId)
        {
            var result = await _scheduleService.CompleteScheduleAsync(scheduleId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Batch Operations

        [HttpDelete("batch")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSchedules([FromBody] List<Guid> ids, [FromQuery] bool isPermanent = false)
        {
            var result = await _scheduleService.DeleteSchedulesAsync(ids, isPermanent);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("batch/restore")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreSchedules([FromBody] List<Guid> ids)
        {
            var result = await _scheduleService.RestoreSchedulesAsync(ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Soft Delete Operations

        [HttpGet("deleted")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSoftDeletedSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _scheduleService.GetSoftDeletedSchedulesAsync(pageNumber, pageSize, searchTerm);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}