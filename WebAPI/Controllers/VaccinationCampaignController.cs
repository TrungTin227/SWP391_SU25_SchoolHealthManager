using DTOs.VaccinationCampaignDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ValidateModel]
    public class VaccinationCampaignController : ControllerBase
    {
        private readonly IVaccinationCampaignService _vaccinationCampaignService;

        public VaccinationCampaignController(IVaccinationCampaignService vaccinationCampaignService)
        {
            _vaccinationCampaignService = vaccinationCampaignService;
        }

        /// <summary>
        /// Lấy danh sách chiến dịch tiêm chủng với phân trang và bộ lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVaccinationCampaigns(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] VaccinationCampaignStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _vaccinationCampaignService.GetVaccinationCampaignsAsync(
                pageNumber, pageSize, searchTerm, status, startDate, endDate);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết chiến dịch tiêm chủng theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVaccinationCampaignById(Guid id)
        {
            var result = await _vaccinationCampaignService.GetVaccinationCampaignByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết đầy đủ chiến dịch tiêm chủng theo ID
        /// </summary>
        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetVaccinationCampaignDetailById(Guid id)
        {
            var result = await _vaccinationCampaignService.GetVaccinationCampaignDetailByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Tạo chiến dịch tiêm chủng mới (Trạng thái: Pending)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVaccinationCampaign([FromBody] CreateVaccinationCampaignRequest request)
        {
            var result = await _vaccinationCampaignService.CreateVaccinationCampaignAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật thông tin chiến dịch tiêm chủng
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVaccinationCampaign(Guid id, [FromBody] UpdateVaccinationCampaignRequest request)
        {
            if (id != request.Id)
                return BadRequest(new { Message = "ID trong URL và body không khớp" });

            var result = await _vaccinationCampaignService.UpdateVaccinationCampaignAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #region Status Management APIs (Workflow: Pending → InProgress → Resolved)

        /// <summary>
        /// Bắt đầu chiến dịch tiêm chủng (Pending → InProgress)
        /// </summary>
        [HttpPatch("{id:guid}/start")]
        public async Task<IActionResult> StartCampaign(Guid id, [FromBody] string? notes = null)
        {
            var result = await _vaccinationCampaignService.StartCampaignAsync(id, notes);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Hoàn thành chiến dịch tiêm chủng (InProgress → Resolved)
        /// </summary>
        [HttpPatch("{id:guid}/complete")]
        public async Task<IActionResult> CompleteCampaign(Guid id, [FromBody] string? notes = null)
        {
            var result = await _vaccinationCampaignService.CompleteCampaignAsync(id, notes);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Hủy chiến dịch tiêm chủng (Pending/InProgress → Cancelled)
        /// </summary>
        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> CancelCampaign(Guid id, [FromBody] string? notes = null)
        {
            var result = await _vaccinationCampaignService.CancelCampaignAsync(id, notes);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Batch Operations APIs

        /// <summary>
        /// Xóa mềm nhiều chiến dịch tiêm chủng cùng lúc
        /// </summary>
        [HttpDelete("batch")]
        public async Task<IActionResult> BatchSoftDeleteCampaigns([FromBody] BatchVaccinationCampaignRequest request)
        {
            var result = await _vaccinationCampaignService.SoftDeleteCampaignsAsync(request.CampaignIds);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Khôi phục nhiều chiến dịch tiêm chủng đã xóa cùng lúc
        /// </summary>
        [HttpPatch("batch/restore")]
        public async Task<IActionResult> BatchRestoreCampaigns([FromBody] BatchVaccinationCampaignRequest request)
        {
            var result = await _vaccinationCampaignService.RestoreCampaignsAsync(request.CampaignIds);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật trạng thái nhiều chiến dịch tiêm chủng cùng lúc
        /// </summary>
        [HttpPatch("batch/status")]
        public async Task<IActionResult> BatchUpdateCampaignStatus([FromBody] BatchUpdateCampaignStatusRequest request)
        {
            var result = await _vaccinationCampaignService.BatchUpdateCampaignStatusAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Soft Delete Management APIs

        /// <summary>
        /// Lấy danh sách chiến dịch tiêm chủng đã bị xóa mềm
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedCampaigns(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _vaccinationCampaignService.GetSoftDeletedCampaignsAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}