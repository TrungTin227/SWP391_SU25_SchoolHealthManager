using DTOs.MedicalSupplyLotDTOs.Request;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebAPI.Middlewares;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MedicalSupplyLotController : ControllerBase
    {
        private readonly IMedicalSupplyLotService _medicalSupplyLotService;

        public MedicalSupplyLotController(IMedicalSupplyLotService medicalSupplyLotService)
        {
            _medicalSupplyLotService = medicalSupplyLotService;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy danh sách lô vật tư y tế theo phân trang với khả năng tìm kiếm và lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedicalSupplyLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? medicalSupplyId = null,
            [FromQuery] bool? isExpired = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _medicalSupplyLotService.GetMedicalSupplyLotsAsync(
                pageNumber, pageSize, searchTerm, medicalSupplyId, isExpired);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết lô vật tư y tế theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicalSupplyLotById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID lô vật tư y tế không hợp lệ");

            var result = await _medicalSupplyLotService.GetMedicalSupplyLotByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Tạo mới một lô vật tư y tế
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMedicalSupplyLot([FromBody] CreateMedicalSupplyLotRequest request)
        {
            var result = await _medicalSupplyLotService.CreateMedicalSupplyLotAsync(request);

            return result.IsSuccess ? CreatedAtAction(
                nameof(GetMedicalSupplyLotById),
                new { id = result.Data?.Id },
                result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật thông tin lô vật tư y tế
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicalSupplyLot(Guid id, [FromBody] UpdateMedicalSupplyLotRequest request)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID lô vật tư y tế không hợp lệ");

            var result = await _medicalSupplyLotService.UpdateMedicalSupplyLotAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Unified Delete & Restore Operations

        /// <summary>
        /// Xóa lô vật tư y tế (hỗ trợ cả xóa mềm và xóa vĩnh viễn, cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteMedicalSupplyLots([FromBody] DeleteMedicalSupplyLotsRequest request)
        {
            // Validate request
            ValidateBatchRequest(request, request.IsPermanent ? 50 : 100,
                request.IsPermanent ? "xóa vĩnh viễn" : "xóa");

            // Execute unified delete operation
            var result = await _medicalSupplyLotService.DeleteMedicalSupplyLotsAsync(request.Ids, request.IsPermanent);

            return HandleBatchOperationResult(result);
        }

        /// <summary>
        /// Khôi phục lô vật tư y tế (hỗ trợ cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreMedicalSupplyLots([FromBody] RestoreMedicalSupplyLotsRequest request)
        {
            ValidateBatchRequest(request, 100, "khôi phục");

            var result = await _medicalSupplyLotService.RestoreMedicalSupplyLotsAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Soft Delete Operations

        /// <summary>
        /// Lấy danh sách lô vật tư y tế đã bị xóa mềm (Chỉ Admin)
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedMedicalSupplyLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _medicalSupplyLotService.GetSoftDeletedLotsAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// Lấy danh sách lô vật tư y tế sắp hết hạn
        /// </summary>
        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringMedicalSupplyLots(
            [FromQuery][Range(1, 365)] int daysBeforeExpiry = 30)
        {
            var result = await _medicalSupplyLotService.GetExpiringLotsAsync(daysBeforeExpiry);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách lô vật tư y tế đã hết hạn
        /// </summary>
        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredMedicalSupplyLots()
        {
            var result = await _medicalSupplyLotService.GetExpiredLotsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy tất cả lô vật tư y tế của một vật tư cụ thể
        /// </summary>
        [HttpGet("by-supply/{medicalSupplyId:guid}")]
        public async Task<IActionResult> GetLotsByMedicalSupplyId(Guid medicalSupplyId)
        {
            if (medicalSupplyId == Guid.Empty)
                throw new ArgumentException("ID vật tư y tế không hợp lệ");

            var result = await _medicalSupplyLotService.GetLotsByMedicalSupplyIdAsync(medicalSupplyId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy số lượng có sẵn của một vật tư y tế cụ thể
        /// </summary>
        [HttpGet("available-quantity/{medicalSupplyId:guid}")]
        public async Task<IActionResult> GetAvailableQuantity(Guid medicalSupplyId)
        {
            if (medicalSupplyId == Guid.Empty)
                throw new ArgumentException("ID vật tư y tế không hợp lệ");

            var result = await _medicalSupplyLotService.GetAvailableQuantityAsync(medicalSupplyId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật số lượng cho lô vật tư y tế cụ thể
        /// </summary>
        [HttpPatch("{id:guid}/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            ValidateQuantityUpdate(id, request.Quantity);

            var result = await _medicalSupplyLotService.UpdateQuantityAsync(id, request.Quantity);
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
                throw new ArgumentException($"Không thể {operation} quá {maxItems} lô vật tư y tế cùng lúc");

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
        /// Validate quantity update parameters
        /// </summary>
        private static void ValidateQuantityUpdate(Guid id, int quantity)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID lô vật tư y tế không hợp lệ");

            if (quantity < 0)
                throw new ArgumentException("Số lượng không được âm");
        }

        #endregion
    }
}