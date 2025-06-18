using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    //[Authorize] // Yêu cầu xác thực cho tất cả endpoints
    public class MedicationLotController : ControllerBase
    {
        private readonly IMedicationLotService _medicationLotService;

        public MedicationLotController(IMedicationLotService medicationLotService)
        {
            _medicationLotService = medicationLotService;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy danh sách lô thuốc theo phân trang với khả năng tìm kiếm và lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedicationLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? medicationId = null,
            [FromQuery] bool? isExpired = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _medicationLotService.GetMedicationLotsAsync(
                pageNumber, pageSize, searchTerm, medicationId, isExpired);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết lô thuốc theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicationLotById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID lô thuốc không hợp lệ");

            var result = await _medicationLotService.GetMedicationLotByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Tạo mới một lô thuốc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMedicationLot([FromBody] CreateMedicationLotRequest request)
        {
            var result = await _medicationLotService.CreateMedicationLotAsync(request);

            return result.IsSuccess ? CreatedAtAction(
                nameof(GetMedicationLotById),
                new { id = result.Data?.Id },
                result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật thông tin lô thuốc
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicationLot(Guid id, [FromBody] UpdateMedicationLotRequest request)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID lô thuốc không hợp lệ");

            var result = await _medicationLotService.UpdateMedicationLotAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Xóa một hoặc nhiều lô thuốc cùng lúc (soft delete hoặc permanent delete)
        /// Sử dụng: {"ids": ["id1"], "isPermanent": false} cho soft delete
        /// hoặc {"ids": ["id1"], "isPermanent": true} cho permanent delete
        /// </summary>
        [HttpPost("batch/delete")]
        public async Task<IActionResult> DeleteMedicationLotsBatch([FromBody] DeleteMedicationLotsRequest request)
        {
            var maxItems = request.IsPermanent ? 50 : 100;
            var operation = request.IsPermanent ? "xóa vĩnh viễn" : "xóa";

            ValidateBatchRequest(request, maxItems, operation);

            var result = await _medicationLotService.DeleteMedicationLotsAsync(request.Ids, request.IsPermanent);

            return HandleBatchOperationResult(result);
        }

        /// <summary>
        /// Khôi phục một hoặc nhiều lô thuốc cùng lúc
        /// Sử dụng: {"ids": ["id1"]} cho 1 lô thuốc hoặc {"ids": ["id1", "id2", "id3"]} cho nhiều lô thuốc
        /// </summary>
        [HttpPost("batch/restore")]
        public async Task<IActionResult> RestoreMedicationLotsBatch([FromBody] RestoreMedicationLotsRequest request)
        {
            ValidateBatchRequest(request, maxItems: 100, operation: "khôi phục");

            var result = await _medicationLotService.RestoreMedicationLotsAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Soft Delete Operations

        /// <summary>
        /// Lấy danh sách lô thuốc đã bị xóa mềm (Chỉ Admin)
        /// </summary>
        [HttpGet("deleted")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetSoftDeletedMedicationLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _medicationLotService.GetSoftDeletedLotsAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Dọn dẹp các lô thuốc đã hết hạn quá thời hạn (Chỉ Admin)
        /// </summary>
        [HttpPost("cleanup")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CleanupExpiredMedicationLots(
            [FromQuery][Range(1, 365)] int daysToExpire = 90)
        {
            var result = await _medicationLotService.CleanupExpiredLotsAsync(daysToExpire);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// Lấy danh sách lô thuốc sắp hết hạn
        /// </summary>
        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringMedicationLots(
            [FromQuery][Range(1, 365)] int daysBeforeExpiry = 30)
        {
            var result = await _medicationLotService.GetExpiringLotsAsync(daysBeforeExpiry);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách lô thuốc đã hết hạn
        /// </summary>
        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredMedicationLots()
        {
            var result = await _medicationLotService.GetExpiredLotsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy tất cả lô thuốc của một loại thuốc cụ thể
        /// </summary>
        [HttpGet("by-medication/{medicationId:guid}")]
        public async Task<IActionResult> GetLotsByMedicationId(Guid medicationId)
        {
            if (medicationId == Guid.Empty)
                throw new ArgumentException("ID thuốc không hợp lệ");

            var result = await _medicationLotService.GetLotsByMedicationIdAsync(medicationId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy số lượng có sẵn của một loại thuốc cụ thể
        /// </summary>
        [HttpGet("available-quantity/{medicationId:guid}")]
        public async Task<IActionResult> GetAvailableQuantity(Guid medicationId)
        {
            if (medicationId == Guid.Empty)
                throw new ArgumentException("ID thuốc không hợp lệ");

            var result = await _medicationLotService.GetAvailableQuantityAsync(medicationId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật số lượng cho lô thuốc cụ thể
        /// </summary>
        [HttpPatch("{id:guid}/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            ValidateQuantityUpdate(id, request.Quantity);

            var result = await _medicationLotService.UpdateQuantityAsync(id, request.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Lấy thống kê lô thuốc (bao gồm cả thông tin chi tiết và tóm tắt)
        /// Frontend có thể tự xử lý để hiển thị summary hoặc chi tiết
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetMedicationLotStatistics()
        {
            var result = await _medicationLotService.GetStatisticsAsync();
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
                throw new ArgumentException($"Không thể {operation} quá {maxItems} lô thuốc cùng lúc");

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
                throw new ArgumentException("ID lô thuốc không hợp lệ");

            if (quantity < 0)
                throw new ArgumentException("Số lượng không được âm");
        }

        #endregion
    }
}