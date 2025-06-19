using DTOs.VaccinationScheduleDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VaccinationScheduleController : ControllerBase
    {
        private readonly IVaccinationScheduleService _scheduleService;

        public VaccinationScheduleController(IVaccinationScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        #region CRUD Operations

        [HttpPost]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateVaccinationScheduleRequest request)
        {
            var result = await _scheduleService.CreateScheduleAsync(request);
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

        [HttpGet("campaign/{campaignId}")]
        public async Task<IActionResult> GetSchedulesByCampaign(
            Guid campaignId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _scheduleService.GetSchedulesByCampaignAsync(
                campaignId, pageNumber, pageSize, searchTerm);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetSchedulesByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _scheduleService.GetSchedulesByDateRangeAsync(
                startDate, endDate, pageNumber, pageSize, searchTerm);
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

        [HttpPatch("{scheduleId}/status")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> UpdateScheduleStatus(Guid scheduleId, [FromBody] ScheduleStatus newStatus)
        {
            var result = await _scheduleService.UpdateScheduleStatusAsync(scheduleId, newStatus);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

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

        #region Business Operations

        [HttpGet("pending")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> GetPendingSchedules()
        {
            var result = await _scheduleService.GetPendingSchedulesAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("in-progress")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> GetInProgressSchedules()
        {
            var result = await _scheduleService.GetInProgressSchedulesAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}