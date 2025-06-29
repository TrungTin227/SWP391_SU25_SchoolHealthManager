using DTOs.VaccinationRecordDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class VaccinationRecordController : ControllerBase
    {
        private readonly IVaccinationRecordService _vaccinationRecordService;

        public VaccinationRecordController(IVaccinationRecordService recordService)
        {
            _vaccinationRecordService = recordService;
        }

        // POST: api/vaccinationrecord
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVaccinationRecordRequest request)
        {
            var result = await _vaccinationRecordService.CreateAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // PUT: api/vaccinationrecord/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaccinationRecordRequest request)
        {
            var result = await _vaccinationRecordService.UpdateAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // DELETE: api/vaccinationrecord/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid deletedBy)
        {
            var result = await _vaccinationRecordService.DeleteAsync(id, deletedBy);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // GET: api/vaccinationrecord/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _vaccinationRecordService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // GET: api/vaccinationrecord/schedule/{scheduleId}
        [HttpGet("schedule/{scheduleId}")]
        public async Task<IActionResult> GetBySchedule(Guid scheduleId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            var result = await _vaccinationRecordService.GetRecordsByScheduleAsync(scheduleId, pageNumber, pageSize, searchTerm);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // GET: api/vaccinationrecord/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            var result = await _vaccinationRecordService.GetRecordsByStudentAsync(studentId, pageNumber, pageSize, searchTerm);
            return Ok(result);
        }
    }
}
