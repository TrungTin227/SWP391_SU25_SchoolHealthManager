using DTOs.CheckupCampaign.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CheckupCampaignController : ControllerBase
    {
        private readonly ICheckupCampaignService _checkupCampaignService;

        public CheckupCampaignController(
            ICheckupCampaignService checkupCampaignService)
        {
            _checkupCampaignService = checkupCampaignService;
        }

        /// <summary>
        /// Lấy danh sách chiến dịch khám định kỳ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCheckupCampaigns(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] CheckupCampaignStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? id = null)
        {
            if (id.HasValue)
            {
                var single = await _checkupCampaignService.GetCheckupCampaignByIdAsync(id.Value);
                return single.IsSuccess ? Ok(single) : NotFound(single);
            }

            var result = await _checkupCampaignService.GetCheckupCampaignsAsync(
                pageNumber, pageSize, searchTerm, status, startDate, endDate);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy chi tiết chiến dịch khám định kỳ theo ID
        /// </summary>
        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetCheckupCampaignDetail(Guid id)
        {
            var result = await _checkupCampaignService.GetCheckupCampaignDetailByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Tạo chiến dịch khám định kỳ mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCheckupCampaign([FromBody] CreateCheckupCampaignRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupCampaignService.CreateCheckupCampaignAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật chiến dịch khám định kỳ
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCheckupCampaign(Guid id, [FromBody] UpdateCheckupCampaignRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.Id)
                return BadRequest("ID không khớp");

            var result = await _checkupCampaignService.UpdateCheckupCampaignAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Bắt đầu chiến dịch khám định kỳ
        /// </summary>
        [HttpPost("{id:guid}/start")]
        public async Task<IActionResult> StartCampaign(Guid id, [FromBody] string? notes = null)
        {
            var result = await _checkupCampaignService.StartCampaignAsync(id, notes);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Hoàn thành chiến dịch khám định kỳ
        /// </summary>
        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> CompleteCampaign(Guid id, [FromBody] string? notes = null)
        {
            var result = await _checkupCampaignService.CompleteCampaignAsync(id, notes);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Hủy chiến dịch khám định kỳ
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelCampaign(Guid id, [FromBody] string? reason = null)
        {
            var result = await _checkupCampaignService.CancelCampaignAsync(id, reason);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật trạng thái hàng loạt
        /// </summary>
        [HttpPost("batch/update-status")]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] DTOs.CheckupCampaign.Request.BatchUpdateCampaignStatusRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupCampaignService.BatchUpdateCampaignStatusAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Xóa mềm hàng loạt
        /// </summary>
        [HttpPost("batch/delete")]
        public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteCampaignRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupCampaignService.BatchDeleteCampaignsAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Khôi phục hàng loạt
        /// </summary>
        [HttpPost("batch/restore")]
        public async Task<IActionResult> BatchRestore([FromBody] BatchRestoreCampaignRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkupCampaignService.BatchRestoreCampaignsAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Thống kê trạng thái chiến dịch
        /// </summary>
        [HttpGet("statistics/status")]
        public async Task<IActionResult> GetStatusStatistics()
        {
            var result = await _checkupCampaignService.GetCampaignStatusStatisticsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedCampaigns(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
        {
            var result = await _checkupCampaignService.GetSoftDeletedCampaignsAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}