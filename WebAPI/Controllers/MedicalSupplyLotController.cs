using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        /// Lấy danh sách lô vật tư y tế theo phân trang với tìm kiếm, lọc theo supply, hết hạn và deleted
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedicalSupplyLots(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? medicalSupplyId = null,
            [FromQuery] bool? isExpired = null,
            [FromQuery] bool includeDeleted = false)
        {
            if (pageNumber < 1) return BadRequest("Số trang phải lớn hơn 0");

            var result = await _medicalSupplyLotService.GetMedicalSupplyLotsAsync(
                pageNumber, pageSize, searchTerm, medicalSupplyId, isExpired, includeDeleted);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy chi tiết một lô theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicalSupplyLotById(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("ID lô vật tư y tế không hợp lệ");

            var result = await _medicalSupplyLotService.GetMedicalSupplyLotByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        #endregion

        #region Supply-specific Operations

        /// <summary>
        /// Lấy lô của một supply, có thể kèm tổng quantity
        /// </summary>
        [HttpGet("by-supply/{medicalSupplyId:guid}")]
        public async Task<IActionResult> GetLotsByMedicalSupplyId(
            Guid medicalSupplyId,
            [FromQuery] bool includeQuantitySummary = false)
        {
            if (medicalSupplyId == Guid.Empty)
                return BadRequest("ID vật tư y tế không hợp lệ");

            if (includeQuantitySummary)
            {
                var lots = await _medicalSupplyLotService.GetLotsByMedicalSupplyIdAsync(medicalSupplyId);
                var qty = await _medicalSupplyLotService.GetAvailableQuantityAsync(medicalSupplyId);

                if (lots.IsSuccess && qty.IsSuccess)
                {
                    var data = new
                    {
                        Lots = lots.Data,
                        AvailableQuantity = qty.Data
                    };
                    return Ok(ApiResult<object>.Success(data, "Thành công"));
                }
                return BadRequest(ApiResult<object>.Failure(new Exception("Không thể lấy dữ liệu")));
            }

            var result = await _medicalSupplyLotService.GetLotsByMedicalSupplyIdAsync(medicalSupplyId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Expiry Operations

        /// <summary>
        /// Lấy lô theo trạng thái expiry: expiring, expired, all
        /// </summary>
        [HttpGet("expiry-status")]
        public async Task<IActionResult> GetLotsByExpiryStatus(
    [FromQuery] string status = "expiring",
    [FromQuery][Range(1, 365)] int daysBeforeExpiry = 30)
        {
            var statusLower = status.ToLower();

            if (statusLower == "expired")
            {
                var result = await _medicalSupplyLotService.GetExpiredLotsAsync();
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            else if (statusLower == "expiring")
            {
                var result = await _medicalSupplyLotService.GetExpiringLotsAsync(daysBeforeExpiry);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            else if (statusLower == "all")
            {
                var result = await GetAllExpiryLots(daysBeforeExpiry);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            else
            {
                var errorResult = ApiResult<object>.Failure(new ArgumentException("Status phải là: expiring, expired hoặc all"));
                return BadRequest(errorResult);
            }
        }

        private async Task<ApiResult<object>> GetAllExpiryLots(int daysBeforeExpiry)
        {
            var expiring = await _medicalSupplyLotService.GetExpiringLotsAsync(daysBeforeExpiry);
            var expired = await _medicalSupplyLotService.GetExpiredLotsAsync();

            if (expiring.IsSuccess && expired.IsSuccess)
            {
                var data = new
                {
                    Expiring = expiring.Data,
                    Expired = expired.Data
                };
                return ApiResult<object>.Success(data, "Thành công");
            }

            return ApiResult<object>.Failure(new Exception("Không thể lấy dữ liệu"));
        }

        #endregion

        #region Create/Update/Delete

        [HttpPost]
        public async Task<IActionResult> CreateMedicalSupplyLot([FromBody] CreateMedicalSupplyLotRequest request)
        {
            var result = await _medicalSupplyLotService.CreateMedicalSupplyLotAsync(request);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetMedicalSupplyLotById), new { id = result.Data?.Id }, result)
                : BadRequest(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicalSupplyLot(Guid id, [FromBody] UpdateMedicalSupplyLotRequest request)
        {
            if (id == Guid.Empty) return BadRequest("ID lô vật tư y tế không hợp lệ");

            var result = await _medicalSupplyLotService.UpdateMedicalSupplyLotAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id:guid}/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateQuantityRequest request)
        {
            if (id == Guid.Empty) return BadRequest("ID lô vật tư y tế không hợp lệ");
            if (request.Quantity < 0) return BadRequest("Số lượng không được âm");

            var result = await _medicalSupplyLotService.UpdateQuantityAsync(id, request.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMedicalSupplyLots([FromBody] DeleteMedicalSupplyLotsRequest request)
        {
            if (!ValidateBatchRequest(request?.Ids, request?.IsPermanent ?? false, "xóa", out var err))
                return BadRequest(err);

            var result = await _medicalSupplyLotService.DeleteMedicalSupplyLotsAsync(request.Ids, request.IsPermanent);
            return HandleBatchOperationResult(result);
        }

        [HttpPost("restore")]
        public async Task<IActionResult> RestoreMedicalSupplyLots([FromBody] RestoreMedicalSupplyLotsRequest request)
        {
            if (!ValidateBatchRequest(request?.Ids, false, "khôi phục", out var err))
                return BadRequest(err);

            var result = await _medicalSupplyLotService.RestoreMedicalSupplyLotsAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Helpers

        private static bool ValidateBatchRequest(List<Guid>? ids, bool isPermanent, string op, out string error)
        {
            error = string.Empty;
            if (ids == null || !ids.Any()) { error = "Danh sách ID không được rỗng"; return false; }
            var max = isPermanent ? 50 : 100;
            if (ids.Count > max) { error = $"Không thể {op} quá {max} lô"; return false; }
            if (ids.Any(id => id == Guid.Empty)) { error = "Danh sách chứa ID không hợp lệ"; return false; }
            return true;
        }

        private IActionResult HandleBatchOperationResult(dynamic result)
        {
            if (result.Data is BatchOperationResultDTO b)
                return b.IsCompleteSuccess
                    ? Ok(result)
                    : b.IsPartialSuccess
                        ? StatusCode(207, result)
                        : BadRequest(result);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}
