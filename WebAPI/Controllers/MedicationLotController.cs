using DTOs.Common;
using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Yêu cầu xác thực cho tất cả endpoints
    public class MedicationLotController : ControllerBase
    {
        private readonly IMedicationLotService _medicationLotService;
        private readonly ILogger<MedicationLotController> _logger;

        public MedicationLotController(
            IMedicationLotService medicationLotService,
            ILogger<MedicationLotController> logger)
        {
            _medicationLotService = medicationLotService;
            _logger = logger;
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
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("Số trang phải lớn hơn 0");
                }

                var result = await _medicationLotService.GetMedicationLotsAsync(
                    pageNumber, pageSize, searchTerm, medicationId, isExpired);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationLots");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết lô thuốc theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicationLotById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID lô thuốc không hợp lệ");
                }

                var result = await _medicationLotService.GetMedicationLotByIdAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationLotById for ID: {MedicationLotId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Tạo mới một lô thuốc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMedicationLot([FromBody] CreateMedicationLotRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _medicationLotService.CreateMedicationLotAsync(request);

                return result.IsSuccess ? CreatedAtAction(
                    nameof(GetMedicationLotById),
                    new { id = result.Data?.Id },
                    result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateMedicationLot");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Cập nhật thông tin lô thuốc
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicationLot(Guid id, [FromBody] UpdateMedicationLotRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID lô thuốc không hợp lệ");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _medicationLotService.UpdateMedicationLotAsync(id, request);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateMedicationLot for ID: {MedicationLotId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Xóa lô thuốc (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteMedicationLot(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID lô thuốc không hợp lệ");
                }

                var result = await _medicationLotService.DeleteMedicationLotAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteMedicationLot for ID: {MedicationLotId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Xóa nhiều lô thuốc cùng lúc (soft delete)
        /// </summary>
        [HttpPost("batch/delete")]
        public async Task<IActionResult> DeleteMedicationLotsBatch([FromBody] BatchIdsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.Ids == null || !request.Ids.Any())
                {
                    return BadRequest("Danh sách ID không được rỗng");
                }

                if (request.Ids.Count > 100)
                {
                    return BadRequest("Không thể xóa quá 100 lô thuốc cùng lúc");
                }

                var result = await _medicationLotService.DeleteMedicationLotsAsync(request.Ids);

                // Return appropriate status based on batch operation result
                if (result.Data is BatchOperationResultDTO batchResult)
                {
                    if (batchResult.IsCompleteSuccess)
                        return Ok(result);
                    else if (batchResult.IsPartialSuccess)
                        return StatusCode(207, result); // Multi-Status for partial success
                    else
                        return BadRequest(result);
                }

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteMedicationLotsBatch");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Khôi phục nhiều lô thuốc cùng lúc
        /// </summary>
        [HttpPost("batch/restore")]
        public async Task<IActionResult> RestoreMedicationLotsBatch([FromBody] BatchIdsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.Ids == null || !request.Ids.Any())
                {
                    return BadRequest("Danh sách ID không được rỗng");
                }

                if (request.Ids.Count > 100)
                {
                    return BadRequest("Không thể khôi phục quá 100 lô thuốc cùng lúc");
                }

                var result = await _medicationLotService.RestoreMedicationLotsAsync(request.Ids);

                // Return appropriate status based on batch operation result
                if (result.Data is BatchOperationResultDTO batchResult)
                {
                    if (batchResult.IsCompleteSuccess)
                        return Ok(result);
                    else if (batchResult.IsPartialSuccess)
                        return StatusCode(207, result); // Multi-Status for partial success
                    else
                        return BadRequest(result);
                }

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in RestoreMedicationLotsBatch");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Xóa vĩnh viễn nhiều lô thuốc cùng lúc (Chỉ Admin)
        /// </summary>
        [HttpPost("batch/permanent-delete")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> PermanentDeleteMedicationLotsBatch([FromBody] BatchIdsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.Ids == null || !request.Ids.Any())
                {
                    return BadRequest("Danh sách ID không được rỗng");
                }

                if (request.Ids.Count > 50)
                {
                    return BadRequest("Không thể xóa vĩnh viễn quá 50 lô thuốc cùng lúc");
                }

                var result = await _medicationLotService.PermanentDeleteMedicationLotsAsync(request.Ids);

                // Return appropriate status based on batch operation result
                if (result.Data is BatchOperationResultDTO batchResult)
                {
                    if (batchResult.IsCompleteSuccess)
                        return Ok(result);
                    else if (batchResult.IsPartialSuccess)
                        return StatusCode(207, result); // Multi-Status for partial success
                    else
                        return BadRequest(result);
                }

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PermanentDeleteMedicationLotsBatch");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        #endregion

        #region Soft Delete Operations

        /// <summary>
        /// Khôi phục lô thuốc đã bị xóa mềm
        /// </summary>
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> RestoreMedicationLot(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID lô thuốc không hợp lệ");
                }

                var result = await _medicationLotService.RestoreMedicationLotAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in RestoreMedicationLot for ID: {MedicationLotId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Xóa vĩnh viễn lô thuốc (Chỉ Admin)
        /// </summary>
        [HttpDelete("{id:guid}/permanent")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> PermanentDeleteMedicationLot(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID lô thuốc không hợp lệ");
                }

                var result = await _medicationLotService.PermanentDeleteMedicationLotAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PermanentDeleteMedicationLot for ID: {MedicationLotId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

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
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("Số trang phải lớn hơn 0");
                }

                var result = await _medicationLotService.GetSoftDeletedLotsAsync(
                    pageNumber, pageSize, searchTerm);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetSoftDeletedMedicationLots");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Dọn dẹp các lô thuốc đã hết hạn quá thời hạn (Chỉ Admin)
        /// </summary>
        [HttpPost("cleanup")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CleanupExpiredMedicationLots(
            [FromQuery][Range(1, 365)] int daysToExpire = 90)
        {
            try
            {
                var result = await _medicationLotService.CleanupExpiredLotsAsync(daysToExpire);

                // Handle batch operation result for cleanup
                if (result.Data is BatchOperationResultDTO batchResult)
                {
                    if (batchResult.IsCompleteSuccess)
                        return Ok(result);
                    else if (batchResult.IsPartialSuccess)
                        return StatusCode(207, result); // Multi-Status for partial success
                    else if (batchResult.IsCompleteFailure)
                        return BadRequest(result);
                }

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CleanupExpiredMedicationLots");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
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
            try
            {
                var result = await _medicationLotService.GetExpiringLotsAsync(daysBeforeExpiry);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetExpiringMedicationLots");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy danh sách lô thuốc đã hết hạn
        /// </summary>
        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredMedicationLots()
        {
            try
            {
                var result = await _medicationLotService.GetExpiredLotsAsync();

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetExpiredMedicationLots");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy tất cả lô thuốc của một loại thuốc cụ thể
        /// </summary>
        [HttpGet("by-medication/{medicationId:guid}")]
        public async Task<IActionResult> GetLotsByMedicationId(Guid medicationId)
        {
            try
            {
                if (medicationId == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationLotService.GetLotsByMedicationIdAsync(medicationId);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetLotsByMedicationId for MedicationId: {MedicationId}", medicationId);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy số lượng có sẵn của một loại thuốc cụ thể
        /// </summary>
        [HttpGet("available-quantity/{medicationId:guid}")]
        public async Task<IActionResult> GetAvailableQuantity(Guid medicationId)
        {
            try
            {
                if (medicationId == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationLotService.GetAvailableQuantityAsync(medicationId);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAvailableQuantity for MedicationId: {MedicationId}", medicationId);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Cập nhật số lượng cho lô thuốc cụ thể
        /// </summary>
        [HttpPatch("{id:guid}/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID lô thuốc không hợp lệ");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.Quantity < 0)
                {
                    return BadRequest("Số lượng không được âm");
                }

                var result = await _medicationLotService.UpdateQuantityAsync(id, request.Quantity);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateQuantity for ID: {MedicationLotId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Lấy thống kê lô thuốc
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetMedicationLotStatistics()
        {
            try
            {
                var result = await _medicationLotService.GetStatisticsAsync();

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationLotStatistics");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy tóm tắt thống kê theo thời gian thực cho dashboard
        /// </summary>
        [HttpGet("statistics/summary")]
        public async Task<IActionResult> GetStatisticsSummary()
        {
            try
            {
                var result = await _medicationLotService.GetStatisticsAsync();

                if (!result.IsSuccess)
                    return BadRequest(result);

                var summary = new
                {
                    Total = result.Data!.TotalLots,
                    Active = result.Data.ActiveLots,
                    Expired = result.Data.ExpiredLots,
                    Expiring = result.Data.ExpiringInNext30Days,
                    HealthScore = CalculateHealthScore(result.Data),
                    LastUpdated = result.Data.GeneratedAt
                };

                var summaryResult = new
                {
                    IsSuccess = true,
                    Data = summary,
                    Message = "Lấy tóm tắt thống kê thành công"
                };

                return Ok(summaryResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetStatisticsSummary");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Tính toán điểm sức khỏe dựa trên thống kê lô thuốc
        /// </summary>
        private static int CalculateHealthScore(MedicationLotStatisticsResponseDTO stats)
        {
            try
            {
                if (stats.TotalLots == 0) return 100;

                var healthScore = 100 - stats.ExpiredPercentage * 1.5 - stats.ExpiringPercentage * 0.5;
                return Math.Max(0, Math.Min(100, (int)Math.Round(healthScore)));
            }
            catch
            {
                return 0; // Trả về 0 nếu có lỗi trong tính toán
            }
        }

        #endregion    
    }
}