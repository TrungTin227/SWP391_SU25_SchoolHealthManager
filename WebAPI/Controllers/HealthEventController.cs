using DTOs.HealthEventDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthEventController : ControllerBase
    {
        private readonly IHealthEventService _healthEventService;
        private readonly ILogger<HealthEventController> _logger;

        public HealthEventController(
            IHealthEventService healthEventService,
            ILogger<HealthEventController> logger)
        {
            _healthEventService = healthEventService;
            _logger = logger;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy danh sách sự kiện y tế với phân trang và bộ lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHealthEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] EventStatus? status = null,
            [FromQuery] EventType? eventType = null,
            [FromQuery] Guid? studentId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _healthEventService.GetHealthEventsAsync(
                pageNumber, pageSize, searchTerm, status, eventType, studentId, fromDate, toDate);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy chi tiết sự kiện y tế theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHealthEventById(Guid id)
        {
            var result = await _healthEventService.GetHealthEventByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Tạo mới sự kiện y tế (trạng thái tự động là Pending)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateHealthEvent([FromBody] CreateHealthEventRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _healthEventService.CreateHealthEventAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Hoàn thành xử lý sự kiện y tế (chuyển trạng thái từ InProgress sang Resolved)
        /// </summary>
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveHealthEvent(Guid id, [FromBody] ResolveHealthEventRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure the request has the correct HealthEventId
            request.HealthEventId = id;

            var result = await _healthEventService.ResolveHealthEventAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Unified Workflow Operations (Combined Medication + Supply)

        /// <summary>
        /// Cập nhật sự kiện y tế với điều trị (thuốc + vật tư y tế) và trạng thái tự động
        /// </summary>
        [HttpPut("treatment")]
        public async Task<IActionResult> UpdateHealthEventWithTreatment([FromBody] UpdateHealthEventRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _healthEventService.UpdateHealthEventWithTreatmentAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Xóa nhiều sự kiện y tế cùng lúc (hỗ trợ soft delete và permanent delete)
        /// </summary>
        [HttpDelete("batch")]
        public async Task<IActionResult> DeleteHealthEvents(
            [FromBody] List<Guid> ids,
            [FromQuery] bool isPermanent = false)
        {
            if (!ids.Any())
                return BadRequest("Danh sách ID không được rỗng");

            var result = await _healthEventService.DeleteHealthEventsAsync(ids, isPermanent);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Khôi phục nhiều sự kiện y tế đã bị soft delete
        /// </summary>
        [HttpPost("batch/restore")]
        public async Task<IActionResult> RestoreHealthEvents([FromBody] List<Guid> ids)
        {
            if (!ids.Any())
                return BadRequest("Danh sách ID không được rỗng");

            var result = await _healthEventService.RestoreHealthEventsAsync(ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Soft Delete Operations

        /// <summary>
        /// Lấy danh sách sự kiện y tế đã bị soft delete
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _healthEventService.GetSoftDeletedEventsAsync(pageNumber, pageSize, searchTerm);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Lấy thống kê sự kiện y tế
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _healthEventService.GetStatisticsAsync(fromDate, toDate);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}