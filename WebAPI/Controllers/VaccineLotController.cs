using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class VaccineLotController : ControllerBase
    {
        private readonly IVaccineLotService _vaccineLotService;

        public VaccineLotController(IVaccineLotService vaccineLotService)
        {
            _vaccineLotService = vaccineLotService;
        }

        #region Basic CRUD Operations

        [HttpGet]
        public async Task<IActionResult> GetVaccineLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? vaccineTypeId = null,
            [FromQuery] bool? isExpired = null)
        {
            var result = await _vaccineLotService.GetVaccineLotsAsync(
                pageNumber, pageSize, searchTerm, vaccineTypeId, isExpired);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVaccineLotById(Guid id)
        {
            var result = await _vaccineLotService.GetVaccineLotByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVaccineLot([FromBody] CreateVaccineLotRequest request)
        {
            var result = await _vaccineLotService.CreateVaccineLotAsync(request);

            return result.IsSuccess ? CreatedAtAction(
                nameof(GetVaccineLotById),
                new { id = result.Data?.Id },
                result) : BadRequest(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVaccineLot(Guid id, [FromBody] UpdateVaccineLotRequest request)
        {
            var result = await _vaccineLotService.UpdateVaccineLotAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Batch Operations

        [HttpPost("batch/delete")]
        public async Task<IActionResult> DeleteVaccineLotsBatch([FromBody] DeleteVaccineLotsRequest request)
        {
            var result = await _vaccineLotService.DeleteVaccineLotsAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        [HttpPost("batch/restore")]
        public async Task<IActionResult> RestoreVaccineLotsBatch([FromBody] RestoreVaccineLotsRequest request)
        {
            var result = await _vaccineLotService.RestoreVaccineLotsAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Business Operations

        [HttpGet("by-vaccine-type/{vaccineTypeId:guid}")]
        public async Task<IActionResult> GetLotsByVaccineType(Guid vaccineTypeId)
        {
            var result = await _vaccineLotService.GetLotsByVaccineTypeAsync(vaccineTypeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("available-quantity/{vaccineTypeId:guid}")]
        public async Task<IActionResult> GetAvailableVaccineQuantity(Guid vaccineTypeId)
        {
            var result = await _vaccineLotService.GetAvailableVaccineQuantityAsync(vaccineTypeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringVaccineLots(
            [FromQuery][Range(1, 365)] int daysBeforeExpiry = 30)
        {
            var result = await _vaccineLotService.GetExpiringVaccineLotsAsync(daysBeforeExpiry);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredVaccineLots()
        {
            var result = await _vaccineLotService.GetExpiredVaccineLotsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id:guid}/quantity")]
        public async Task<IActionResult> UpdateVaccineQuantity(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            var result = await _vaccineLotService.UpdateVaccineQuantityAsync(id, request.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Soft Delete Operations

        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedVaccineLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _vaccineLotService.GetSoftDeletedVaccineLotsAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Statistics

        [HttpGet("statistics")]
        public async Task<IActionResult> GetVaccineLotStatistics()
        {
            var result = await _vaccineLotService.GetVaccineLotStatisticsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Private Helper Methods

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