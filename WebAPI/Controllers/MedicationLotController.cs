using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicationLotController : ControllerBase
    {
        private readonly IMedicationLotService _medicationLotService;

        public MedicationLotController(IMedicationLotService medicationLotService)
        {
            _medicationLotService = medicationLotService;
        }

        /// <summary>
        /// Get paginated list of medication lots with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedicationLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? medicationId = null,
            [FromQuery] bool? isExpired = null)
        {
            var result = await _medicationLotService.GetMedicationLotsAsync(
                pageNumber, pageSize, searchTerm, medicationId, isExpired);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get a specific medication lot by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicationLotById(Guid id)
        {
            var result = await _medicationLotService.GetMedicationLotByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Create a new medication lot
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMedicationLot([FromBody] CreateMedicationLotRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _medicationLotService.CreateMedicationLotAsync(request);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetMedicationLotById), new { id = result.Data!.Id }, result)
                : BadRequest(result);
        }

        /// <summary>
        /// Update an existing medication lot
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicationLot(Guid id, [FromBody] UpdateMedicationLotRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _medicationLotService.UpdateMedicationLotAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Soft delete a medication lot
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteMedicationLot(Guid id)
        {
            var result = await _medicationLotService.DeleteMedicationLotAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Restore a soft-deleted medication lot
        /// </summary>
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> RestoreMedicationLot(Guid id)
        {
            var result = await _medicationLotService.RestoreMedicationLotAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Permanently delete a medication lot (Admin only)
        /// </summary>
        [HttpDelete("{id:guid}/permanent")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> PermanentDeleteMedicationLot(Guid id)
        {
            var result = await _medicationLotService.PermanentDeleteMedicationLotAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get soft-deleted medication lots (Admin only)
        /// </summary>
        [HttpGet("soft-deleted")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetSoftDeletedLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _medicationLotService.GetSoftDeletedLotsAsync(pageNumber, pageSize, searchTerm);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get medication lots that are expiring soon
        /// </summary>
        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringLots([FromQuery] int daysBeforeExpiry = 30)
        {
            var result = await _medicationLotService.GetExpiringLotsAsync(daysBeforeExpiry);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get medication lots that are already expired
        /// </summary>
        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredLots()
        {
            var result = await _medicationLotService.GetExpiredLotsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all lots for a specific medication
        /// </summary>
        [HttpGet("by-medication/{medicationId:guid}")]
        public async Task<IActionResult> GetLotsByMedicationId(Guid medicationId)
        {
            var result = await _medicationLotService.GetLotsByMedicationIdAsync(medicationId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get available quantity for a specific medication
        /// </summary>
        [HttpGet("available-quantity/{medicationId:guid}")]
        public async Task<IActionResult> GetAvailableQuantity(Guid medicationId)
        {
            var result = await _medicationLotService.GetAvailableQuantityAsync(medicationId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update quantity for a specific lot
        /// </summary>
        [HttpPatch("{id:guid}/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            var result = await _medicationLotService.UpdateQuantityAsync(id, request.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Clean up expired lots (Admin only)
        /// </summary>
        [HttpPost("cleanup-expired")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CleanupExpiredLots([FromQuery] int daysToExpire = 90)
        {
            var result = await _medicationLotService.CleanupExpiredLotsAsync(daysToExpire);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get medication lot statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _medicationLotService.GetStatisticsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get detailed medication lot statistics with custom parameters
        /// </summary>
        [HttpGet("statistics/detailed")]
        public async Task<IActionResult> GetDetailedStatistics(
            [FromQuery] int expiringDays = 30,
            [FromQuery] bool includeDeleted = false)
        {
            if (expiringDays < 1 || expiringDays > 365)
                return BadRequest("Số ngày hết hạn phải trong khoảng từ 1 đến 365");

            if (includeDeleted && !User.IsInRole("ADMIN"))
                return Forbid();

            var result = await _medicationLotService.GetStatisticsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get real-time statistics summary for dashboard
        /// </summary>
        [HttpGet("statistics/summary")]
        public async Task<IActionResult> GetStatisticsSummary()
        {
            var result = await _medicationLotService.GetStatisticsAsync();

            if (!result.IsSuccess)
                return BadRequest(result);

            var summary = new
            {
                total = result.Data!.TotalLots,
                active = result.Data.ActiveLots,
                expired = result.Data.ExpiredLots,
                expiring = result.Data.ExpiringInNext30Days,
                healthScore = CalculateHealthScore(result.Data),
                lastUpdated = result.Data.GeneratedAt
            };

            return Ok(ApiResult<object>.Success(summary, "Lấy tóm tắt thống kê thành công"));
        }

        #region Private Helper Methods

        /// <summary>
        /// Calculate health score based on lot statistics
        /// </summary>
        private static int CalculateHealthScore(MedicationLotStatisticsResponseDTO stats)
        {
            if (stats.TotalLots == 0) return 100;

            var healthScore = 100 - stats.ExpiredPercentage * 1.5 - stats.ExpiringPercentage * 0.5;
            return Math.Max(0, Math.Min(100, (int)Math.Round(healthScore)));
        }

        #endregion
    }
}