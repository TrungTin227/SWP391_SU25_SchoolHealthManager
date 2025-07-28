using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CheckupScheduleController : ControllerBase
    {
        private readonly ICheckupScheduleService _checkupScheduleService;

        public CheckupScheduleController(ICheckupScheduleService checkupScheduleService)
        {
            _checkupScheduleService = checkupScheduleService;
        }

        /// <summary>
        /// Lấy danh sách lịch khám định kỳ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCheckupSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? campaignId = null,
            [FromQuery] CheckupScheduleStatus? status = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? id = null)
        {
            if (id.HasValue)
            {
                var single = await _checkupScheduleService.GetCheckupScheduleByIdAsync(id.Value);
                return single.IsSuccess ? Ok(single) : NotFound(single);
            }

            var result = await _checkupScheduleService.GetCheckupSchedulesAsync(
                pageNumber, pageSize, campaignId, status, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [Route("Student")]
        public async Task<IActionResult> GetCheckupScheduleByStudentId(Guid StudentId)
        {
            var results = await _checkupScheduleService.GetCheckupScheduleByStudentIdAsync(StudentId);
            return results.IsSuccess ? Ok(results) : BadRequest(results);
        }

        /// <summary>
        /// Lấy chi tiết lịch khám theo ID
        /// </summary>
        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetCheckupScheduleDetail(Guid id)
        {
            var result = await _checkupScheduleService.GetCheckupScheduleByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Tạo lịch khám mới (có thể tạo hàng loạt theo khối/lớp)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCheckupSchedules([FromBody] CreateCheckupScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupScheduleService.CreateCheckupSchedulesAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật lịch khám
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCheckupSchedule(Guid id, [FromBody] UpdateCheckupScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.Id)
                return BadRequest("ID không khớp");

            var result = await _checkupScheduleService.UpdateCheckupScheduleAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật trạng thái đồng ý của phụ huynh
        /// </summary>
        [HttpPost("consent")]
        public async Task<IActionResult> UpdateConsentStatus([FromBody] UpdateConsentStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupScheduleService.UpdateConsentStatusAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách lịch khám theo chiến dịch
        /// </summary>
        [HttpGet("campaign/{campaignId:guid}")]
        public async Task<IActionResult> GetSchedulesByCampaign(Guid campaignId)
        {
            var result = await _checkupScheduleService.GetSchedulesByCampaignAsync(campaignId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật trạng thái hàng loạt
        /// </summary>
        [HttpPost("batch/update-status")]
        public async Task<IActionResult> BatchUpdateScheduleStatus([FromBody] CheckupBatchUpdateStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupScheduleService.BatchUpdateScheduleStatusAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Xóa hàng loạt (soft delete)
        /// </summary>
        [HttpPost("batch/delete")]
        public async Task<IActionResult> BatchDeleteSchedules([FromBody] CheckupBatchDeleteScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupScheduleService.BatchDeleteSchedulesAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Khôi phục hàng loạt
        /// </summary>
        [HttpPost("batch/restore")]
        public async Task<IActionResult> BatchRestoreSchedules([FromBody] CheckupBatchRestoreScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupScheduleService.BatchRestoreSchedulesAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my-children")]
        public async Task<IActionResult> GetCheckupSchedulesForMyChildren()
        {
            var result = await _checkupScheduleService.GetSchedulesForParentAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách lịch khám đã xóa mềm
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _checkupScheduleService.GetSoftDeletedSchedulesAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Thống kê trạng thái lịch khám
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetScheduleStatusStatistics([FromQuery] Guid? campaignId = null)
        {
            var result = await _checkupScheduleService.GetScheduleStatusStatisticsAsync(campaignId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

    }
}