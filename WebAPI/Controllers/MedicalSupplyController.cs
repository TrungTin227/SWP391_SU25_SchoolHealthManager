using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MedicalSupplyController : ControllerBase
    {
        private readonly IMedicalSupplyService _medicalSupplyService;

        public MedicalSupplyController(IMedicalSupplyService medicalSupplyService)
        {
            _medicalSupplyService = medicalSupplyService;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy danh sách vật tư y tế theo phân trang với khả năng tìm kiếm và lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedicalSupplies(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool includeDeleted = false)
        {
            if (pageNumber < 1)
                return BadRequest("Số trang phải lớn hơn 0");

            var result = await _medicalSupplyService.GetMedicalSuppliesAsync(
                pageNumber, pageSize, searchTerm, isActive, includeDeleted);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết vật tư y tế theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicalSupplyById(
            Guid id,
            [FromQuery] bool includeLots = false) // Tích hợp chức năng detail
        {
            if (id == Guid.Empty)
                return BadRequest("ID vật tư y tế không hợp lệ");

            if (includeLots)
            {
                var detail = await _medicalSupplyService.GetMedicalSupplyDetailByIdAsync(id);
                return detail.IsSuccess ? Ok(detail) : NotFound(detail);
            }
            var summary = await _medicalSupplyService.GetMedicalSupplyByIdAsync(id);
            return summary.IsSuccess ? Ok(summary) : NotFound(summary);
        }

        /// <summary>
        /// Tạo mới một vật tư y tế
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMedicalSupply([FromBody] CreateMedicalSupplyRequest request)
        {
            var result = await _medicalSupplyService.CreateMedicalSupplyAsync(request);

            return result.IsSuccess ? CreatedAtAction(
                nameof(GetMedicalSupplyById),
                new { id = result.Data?.Id },
                result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật thông tin vật tư y tế
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicalSupply(
            Guid id,
            [FromBody] UpdateMedicalSupplyRequest request)
        {
            if (id == Guid.Empty)
                return BadRequest("ID vật tư y tế không hợp lệ");

            var result = await _medicalSupplyService.UpdateMedicalSupplyAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Delete & Restore Operations

        /// <summary>
        /// Xóa vật tư y tế (hỗ trợ cả xóa mềm và xóa vĩnh viễn, cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteMedicalSupplies([FromBody] DeleteMedicalSuppliesRequest request)
        {
            if (!ValidateBatchRequest(request?.Ids, request?.IsPermanent ?? false, "xóa", out var error))
                return BadRequest(error);

            var result = await _medicalSupplyService.DeleteMedicalSuppliesAsync(request.Ids, request.IsPermanent);
            return HandleBatchOperationResult(result);
        }

        /// <summary>
        /// Khôi phục vật tư y tế (hỗ trợ cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreMedicalSupplies([FromBody] RestoreMedicalSuppliesRequest request)
        {
            if (!ValidateBatchRequest(request?.Ids, false, "khôi phục", out var error))
                return BadRequest(error);

            var result = await _medicalSupplyService.RestoreMedicalSuppliesAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// Lấy danh sách vật tư y tế sắp hết hàng
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockSupplies()
        {
            var result = await _medicalSupplyService.GetLowStockSuppliesAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Private Helper Methods

        private static bool ValidateBatchRequest(List<Guid>? ids, bool isPermanent, string operation, out string error)
        {
            error = string.Empty;

            if (ids == null || !ids.Any())
            {
                error = "Danh sách ID không được rỗng";
                return false;
            }

            var maxItems = isPermanent ? 50 : 100;
            if (ids.Count > maxItems)
            {
                error = $"Không thể {operation} quá {maxItems} vật tư y tế cùng lúc";
                return false;
            }

            if (ids.Any(id => id == Guid.Empty))
            {
                error = "Danh sách chứa ID không hợp lệ";
                return false;
            }

            return true;
        }

        private IActionResult HandleBatchOperationResult(dynamic result)
        {
            if (result.Data is BatchOperationResultDTO batchResult)
            {
                return batchResult.IsCompleteSuccess ? Ok(result) :
                       batchResult.IsPartialSuccess ? StatusCode(207, result) :
                       BadRequest(result);
            }

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}