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
            [FromQuery] bool? isActive = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _medicalSupplyService.GetMedicalSuppliesAsync(
                pageNumber, pageSize, searchTerm, isActive);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết vật tư y tế theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicalSupplyById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID vật tư y tế không hợp lệ");

            var result = await _medicalSupplyService.GetMedicalSupplyByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết vật tư y tế kèm danh sách lô
        /// </summary>
        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetMedicalSupplyDetailById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID vật tư y tế không hợp lệ");

            var result = await _medicalSupplyService.GetMedicalSupplyDetailByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
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
        public async Task<IActionResult> UpdateMedicalSupply(Guid id, [FromBody] UpdateMedicalSupplyRequest request)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID vật tư y tế không hợp lệ");

            var result = await _medicalSupplyService.UpdateMedicalSupplyAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Unified Delete & Restore Operations

        /// <summary>
        /// Xóa vật tư y tế (hỗ trợ cả xóa mềm và xóa vĩnh viễn, cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteMedicalSupplies([FromBody] DeleteMedicalSuppliesRequest request)
        {
            // Validate request
            ValidateBatchRequest(request, request.IsPermanent ? 50 : 100,
                request.IsPermanent ? "xóa vĩnh viễn" : "xóa");

            // Execute unified delete operation
            var result = await _medicalSupplyService.DeleteMedicalSuppliesAsync(request.Ids, request.IsPermanent);

            return HandleBatchOperationResult(result);
        }

        /// <summary>
        /// Khôi phục vật tư y tế (hỗ trợ cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreMedicalSupplies([FromBody] RestoreMedicalSuppliesRequest request)
        {
            ValidateBatchRequest(request, 100, "khôi phục");

            var result = await _medicalSupplyService.RestoreMedicalSuppliesAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Soft Delete Operations

        /// <summary>
        /// Lấy danh sách vật tư y tế đã bị xóa mềm (Chỉ Admin)
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedMedicalSupplies(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _medicalSupplyService.GetSoftDeletedSuppliesAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
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

        /// <summary>
        /// Cập nhật tồn kho hiện tại cho vật tư y tế cụ thể
        /// </summary>
        [HttpPatch("{id:guid}/current-stock")]
        public async Task<IActionResult> UpdateCurrentStock(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            ValidateStockUpdate(id, request.Quantity, "tồn kho");

            var result = await _medicalSupplyService.UpdateCurrentStockAsync(id, request.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật tồn kho tối thiểu cho vật tư y tế cụ thể
        /// </summary>
        [HttpPatch("{id:guid}/minimum-stock")]
        public async Task<IActionResult> UpdateMinimumStock(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            ValidateStockUpdate(id, request.Quantity, "tồn kho tối thiểu");

            var result = await _medicalSupplyService.UpdateMinimumStockAsync(id, request.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Validate batch operation request
        /// </summary>
        private static void ValidateBatchRequest(BatchIdsRequest request, int maxItems, string operation)
        {
            if (request?.Ids == null || !request.Ids.Any())
                throw new ArgumentException("Danh sách ID không được rỗng");

            if (request.Ids.Count > maxItems)
                throw new ArgumentException($"Không thể {operation} quá {maxItems} vật tư y tế cùng lúc");

            if (request.Ids.Any(id => id == Guid.Empty))
                throw new ArgumentException("Danh sách chứa ID không hợp lệ");
        }

        /// <summary>
        /// Handle batch operation result with appropriate status codes
        /// </summary>
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

        /// <summary>
        /// Validate stock update parameters
        /// </summary>
        private static void ValidateStockUpdate(Guid id, int quantity, string stockType)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID vật tư y tế không hợp lệ");

            if (quantity < 0)
                throw new ArgumentException($"Số lượng {stockType} không được âm");
        }

        #endregion
    }
}