using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MedicationLotController : ControllerBase
    {
        private readonly IMedicationLotService _medicationLotService;

        public MedicationLotController(IMedicationLotService medicationLotService)
        {
            _medicationLotService = medicationLotService;
        }

        #region Basic CRUD & Filters

        /// <summary>
        /// Lấy danh sách lô thuốc theo phân trang, tìm kiếm, lọc theo thuốc, trạng thái hết hạn và deleted
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedicationLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? medicationId = null,
            [FromQuery] bool? isExpired = null,
            [FromQuery][Range(1, 365)] int? daysBeforeExpiry = null,
            [FromQuery] bool includeDeleted = false)
        {
            if (pageNumber < 1)
                return BadRequest("Số trang phải lớn hơn 0");

            var result = await _medicationLotService.GetMedicationLotsAsync(
                pageNumber, pageSize, searchTerm, medicationId, isExpired, daysBeforeExpiry, includeDeleted);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy chi tiết một lô thuốc theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicationLotById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("ID lô thuốc không hợp lệ");

            var result = await _medicationLotService.GetMedicationLotByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        #endregion

        #region Create/Update/Delete

        [HttpPost]
        public async Task<IActionResult> CreateMedicationLot([FromBody] CreateMedicationLotRequest request)
        {
            var result = await _medicationLotService.CreateMedicationLotAsync(request);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetMedicationLotById), new { id = result.Data?.Id }, result)
                : BadRequest(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicationLot(Guid id, [FromBody] UpdateMedicationLotRequest request)
        {
            if (id == Guid.Empty)
                return BadRequest("ID lô thuốc không hợp lệ");

            var result = await _medicationLotService.UpdateMedicationLotAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id:guid}/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            if (id == Guid.Empty || request.Quantity < 0)
                return BadRequest("Tham số không hợp lệ");

            var result = await _medicationLotService.UpdateQuantityAsync(id, request.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("batch/delete")]
        public async Task<IActionResult> DeleteMedicationLotsBatch([FromBody] DeleteMedicationLotsRequest request)
        {
            var maxItems = request.IsPermanent ? 50 : 100;
            var operation = request.IsPermanent ? "xóa vĩnh viễn" : "xóa";
            if (!ValidateBatch(request, maxItems, operation, out var err))
                return BadRequest(err);

            var result = await _medicationLotService.DeleteMedicationLotsAsync(request.Ids, request.IsPermanent);
            return HandleBatchResult(result);
        }

        [HttpPost("batch/restore")]
        public async Task<IActionResult> RestoreMedicationLotsBatch([FromBody] RestoreMedicationLotsRequest request)
        {
            if (!ValidateBatch(request, 100, "khôi phục", out var err))
                return BadRequest(err);

            var result = await _medicationLotService.RestoreMedicationLotsAsync(request.Ids);
            return HandleBatchResult(result);
        }

        #endregion

        #region Statistics

        [HttpGet("statistics")]
        public async Task<IActionResult> GetMedicationLotStatistics()
        {
            var result = await _medicationLotService.GetStatisticsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Helpers

        private static bool ValidateBatch(BatchIdsRequest req, int max, string op, out string err)
        {
            err = string.Empty;
            if (req?.Ids == null || req.Ids.Count == 0) { err = "Danh sách ID không được rỗng"; return false; }
            if (req.Ids.Count > max) { err = $"Không thể {op} quá {max} lô"; return false; }
            if (req.Ids.Exists(id => id == Guid.Empty)) { err = "ID không hợp lệ"; return false; }
            return true;
        }

        private IActionResult HandleBatchResult(dynamic result)
        {
            if (result.Data is BatchOperationResultDTO b)
            {
                return b.IsCompleteSuccess ? Ok(result)
                     : b.IsPartialSuccess ? StatusCode(207, result)
                     : BadRequest(result);
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        #endregion
    }
}
