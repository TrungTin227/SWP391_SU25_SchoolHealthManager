using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Helpers;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class MedicationLotController : ControllerBase
    {
        private readonly IMedicationLotService _medicationLotService;
        private readonly ILogger<MedicationLotController> _logger;

        public MedicationLotController(
            IMedicationLotService medicationLotService,
            ILogger<MedicationLotController> logger)
        {
            _medicationLotService = medicationLotService ?? throw new ArgumentNullException(nameof(medicationLotService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get paginated list of medication lots with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedList<MedicationLotResponseDTO>>>> GetMedicationLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? medicationId = null,
            [FromQuery] bool? isExpired = null)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(
                        new ArgumentException("Invalid pagination parameters")));
                }

                var result = await _medicationLotService.GetMedicationLotsAsync(
                    pageNumber, pageSize, searchTerm, medicationId, isExpired);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMedicationLots endpoint");
                return StatusCode(500, ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(ex));
            }
        }

        /// <summary>
        /// Get a specific medication lot by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResult<MedicationLotResponseDTO>>> GetMedicationLotById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(ApiResult<MedicationLotResponseDTO>.Failure(
                        new ArgumentException("Invalid lot ID")));
                }

                var result = await _medicationLotService.GetMedicationLotByIdAsync(id);

                if (!result.IsSuccess)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMedicationLotById endpoint for ID: {LotId}", id);
                return StatusCode(500, ApiResult<MedicationLotResponseDTO>.Failure(ex));
            }
        }

        /// <summary>
        /// Create a new medication lot
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResult<MedicationLotResponseDTO>>> CreateMedicationLot(
            [FromBody] CreateMedicationLotRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResult<MedicationLotResponseDTO>.Failure(
                        new ArgumentException("Invalid request data")));
                }

                var result = await _medicationLotService.CreateMedicationLotAsync(request);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(
                    nameof(GetMedicationLotById),
                    new { id = result.Data!.Id },
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateMedicationLot endpoint");
                return StatusCode(500, ApiResult<MedicationLotResponseDTO>.Failure(ex));
            }
        }

        /// <summary>
        /// Update an existing medication lot
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResult<MedicationLotResponseDTO>>> UpdateMedicationLot(
            Guid id,
            [FromBody] UpdateMedicationLotRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(ApiResult<MedicationLotResponseDTO>.Failure(
                        new ArgumentException("Invalid lot ID")));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResult<MedicationLotResponseDTO>.Failure(
                        new ArgumentException("Invalid request data")));
                }

                var result = await _medicationLotService.UpdateMedicationLotAsync(id, request);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateMedicationLot endpoint for ID: {LotId}", id);
                return StatusCode(500, ApiResult<MedicationLotResponseDTO>.Failure(ex));
            }
        }

        /// <summary>
        /// Soft delete a medication lot
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResult<bool>>> DeleteMedicationLot(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(ApiResult<bool>.Failure(
                        new ArgumentException("Invalid lot ID")));
                }

                var result = await _medicationLotService.DeleteMedicationLotAsync(id);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteMedicationLot endpoint for ID: {LotId}", id);
                return StatusCode(500, ApiResult<bool>.Failure(ex));
            }
        }

        /// <summary>
        /// Restore a soft-deleted medication lot
        /// </summary>
        [HttpPost("{id:guid}/restore")]
        public async Task<ActionResult<ApiResult<MedicationLotResponseDTO>>> RestoreMedicationLot(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(ApiResult<MedicationLotResponseDTO>.Failure(
                        new ArgumentException("Invalid lot ID")));
                }

                var result = await _medicationLotService.RestoreMedicationLotAsync(id);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RestoreMedicationLot endpoint for ID: {LotId}", id);
                return StatusCode(500, ApiResult<MedicationLotResponseDTO>.Failure(ex));
            }
        }

        /// <summary>
        /// Permanently delete a medication lot (Admin only)
        /// </summary>
        [HttpDelete("{id:guid}/permanent")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResult<bool>>> PermanentDeleteMedicationLot(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(ApiResult<bool>.Failure(
                        new ArgumentException("Invalid lot ID")));
                }

                var result = await _medicationLotService.PermanentDeleteMedicationLotAsync(id);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PermanentDeleteMedicationLot endpoint for ID: {LotId}", id);
                return StatusCode(500, ApiResult<bool>.Failure(ex));
            }
        }

        /// <summary>
        /// Get soft-deleted medication lots (Admin only)
        /// </summary>
        [HttpGet("soft-deleted")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResult<PagedList<MedicationLotResponseDTO>>>> GetSoftDeletedLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(
                        new ArgumentException("Invalid pagination parameters")));
                }

                var result = await _medicationLotService.GetSoftDeletedLotsAsync(
                    pageNumber, pageSize, searchTerm);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSoftDeletedLots endpoint");
                return StatusCode(500, ApiResult<PagedList<MedicationLotResponseDTO>>.Failure(ex));
            }
        }

        /// <summary>
        /// Get medication lots that are expiring soon
        /// </summary>
        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResult<List<MedicationLotResponseDTO>>>> GetExpiringLots(
            [FromQuery] int daysBeforeExpiry = 30)
        {
            try
            {
                if (daysBeforeExpiry < 0 || daysBeforeExpiry > 365)
                {
                    return BadRequest(ApiResult<List<MedicationLotResponseDTO>>.Failure(
                        new ArgumentException("Days before expiry must be between 0 and 365")));
                }

                var result = await _medicationLotService.GetExpiringLotsAsync(daysBeforeExpiry);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetExpiringLots endpoint");
                return StatusCode(500, ApiResult<List<MedicationLotResponseDTO>>.Failure(ex));
            }
        }

        /// <summary>
        /// Get medication lots that are already expired
        /// </summary>
        [HttpGet("expired")]
        public async Task<ActionResult<ApiResult<List<MedicationLotResponseDTO>>>> GetExpiredLots()
        {
            try
            {
                var result = await _medicationLotService.GetExpiredLotsAsync();

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetExpiredLots endpoint");
                return StatusCode(500, ApiResult<List<MedicationLotResponseDTO>>.Failure(ex));
            }
        }

        /// <summary>
        /// Get all lots for a specific medication
        /// </summary>
        [HttpGet("by-medication/{medicationId:guid}")]
        public async Task<ActionResult<ApiResult<List<MedicationLotResponseDTO>>>> GetLotsByMedicationId(Guid medicationId)
        {
            try
            {
                if (medicationId == Guid.Empty)
                {
                    return BadRequest(ApiResult<List<MedicationLotResponseDTO>>.Failure(
                        new ArgumentException("Invalid medication ID")));
                }

                var result = await _medicationLotService.GetLotsByMedicationIdAsync(medicationId);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLotsByMedicationId endpoint for medication: {MedicationId}", medicationId);
                return StatusCode(500, ApiResult<List<MedicationLotResponseDTO>>.Failure(ex));
            }
        }

        /// <summary>
        /// Get available quantity for a specific medication
        /// </summary>
        [HttpGet("available-quantity/{medicationId:guid}")]
        public async Task<ActionResult<ApiResult<int>>> GetAvailableQuantity(Guid medicationId)
        {
            try
            {
                if (medicationId == Guid.Empty)
                {
                    return BadRequest(ApiResult<int>.Failure(
                        new ArgumentException("Invalid medication ID")));
                }

                var result = await _medicationLotService.GetAvailableQuantityAsync(medicationId);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAvailableQuantity endpoint for medication: {MedicationId}", medicationId);
                return StatusCode(500, ApiResult<int>.Failure(ex));
            }
        }

        /// <summary>
        /// Update quantity for a specific lot
        /// </summary>
        [HttpPatch("{id:guid}/quantity")]
        public async Task<ActionResult<ApiResult<bool>>> UpdateQuantity(
            Guid id,
            [FromBody] UpdateQuantityRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(ApiResult<bool>.Failure(
                        new ArgumentException("Invalid lot ID")));
                }

                if (request.Quantity < 0)
                {
                    return BadRequest(ApiResult<bool>.Failure(
                        new ArgumentException("Quantity cannot be negative")));
                }

                var result = await _medicationLotService.UpdateQuantityAsync(id, request.Quantity);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateQuantity endpoint for lot: {LotId}", id);
                return StatusCode(500, ApiResult<bool>.Failure(ex));
            }
        }

        /// <summary>
        /// Clean up expired lots (Admin only)
        /// </summary>
        [HttpPost("cleanup-expired")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResult<int>>> CleanupExpiredLots(
            [FromQuery] int daysToExpire = 90)
        {
            try
            {
                if (daysToExpire < 0 || daysToExpire > 365)
                {
                    return BadRequest(ApiResult<int>.Failure(
                        new ArgumentException("Days to expire must be between 0 and 365")));
                }

                var result = await _medicationLotService.CleanupExpiredLotsAsync(daysToExpire);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CleanupExpiredLots endpoint");
                return StatusCode(500, ApiResult<int>.Failure(ex));
            }
        }

        /// <summary>
        /// Get medication lot statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResult<MedicationLotStatisticsResponseDTO>>> GetStatistics()
        {
            try
            {
                // Get various statistics
                var expiringLots = await _medicationLotService.GetExpiringLotsAsync(30);
                var expiredLots = await _medicationLotService.GetExpiredLotsAsync();
                var allLots = await _medicationLotService.GetMedicationLotsAsync(1, int.MaxValue);

                if (!expiringLots.IsSuccess || !expiredLots.IsSuccess || !allLots.IsSuccess)
                {
                    return BadRequest(ApiResult<MedicationLotStatisticsResponseDTO>.Failure(
                        new Exception("Failed to retrieve statistics")));
                }

                var statistics = new MedicationLotStatisticsResponseDTO
                {
                    TotalLots = allLots.Data!.MetaData.TotalCount,
                    ExpiringInNext30Days = expiringLots.Data!.Count,
                    ExpiredLots = expiredLots.Data!.Count,
                    ActiveLots = allLots.Data.MetaData.TotalCount - expiredLots.Data.Count,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(ApiResult<MedicationLotStatisticsResponseDTO>.Success(
                    statistics, "Statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStatistics endpoint");
                return StatusCode(500, ApiResult<MedicationLotStatisticsResponseDTO>.Failure(ex));
            }
        }
    }
}