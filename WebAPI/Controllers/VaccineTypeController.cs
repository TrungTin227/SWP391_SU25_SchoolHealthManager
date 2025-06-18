using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class VaccineTypeController : ControllerBase
    {
        private readonly IVaccineTypeService _vaccineTypeService;
        private readonly ILogger<VaccineTypeController> _logger;

        public VaccineTypeController(
            IVaccineTypeService vaccineTypeService,
            ILogger<VaccineTypeController> logger)
        {
            _vaccineTypeService = vaccineTypeService;
            _logger = logger;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy danh sách loại vaccine với phân trang và bộ lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVaccineTypes(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var result = await _vaccineTypeService.GetVaccineTypesAsync(
                pageNumber, pageSize, searchTerm, isActive);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin loại vaccine theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVaccineTypeById(Guid id)
        {
            var result = await _vaccineTypeService.GetVaccineTypeByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết loại vaccine theo ID
        /// </summary>
        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetVaccineTypeDetailById(Guid id)
        {
            var result = await _vaccineTypeService.GetVaccineTypeDetailByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Tạo mới loại vaccine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVaccineType([FromBody] CreateVaccineTypeRequest request)
        {
            var result = await _vaccineTypeService.CreateVaccineTypeAsync(request);

            return result.IsSuccess ? CreatedAtAction(
                nameof(GetVaccineTypeById),
                new { id = result.Data?.Id },
                result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật loại vaccine
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVaccineType(Guid id, [FromBody] UpdateVaccineTypeRequest request)
        {
            var result = await _vaccineTypeService.UpdateVaccineTypeAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Unified Delete & Restore Operations

        /// <summary>
        /// Xóa loại vaccine (hỗ trợ cả xóa mềm và xóa vĩnh viễn, cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteVaccineTypes([FromBody] DeleteVaccineTypesRequest request)
        {
            // Inline validation
            if (request?.Ids == null || !request.Ids.Any())
                return BadRequest("Danh sách ID không được rỗng");

            // Xác định giới hạn và thông báo dựa trên loại xóa
            int maxItems = request.IsPermanent ? 50 : 100;
            string operation = request.IsPermanent ? "xóa vĩnh viễn" : "xóa";

            if (request.Ids.Count > maxItems)
                return BadRequest($"Không thể {operation} quá {maxItems} mục cùng lúc");

            if (request.Ids.Any(id => id == Guid.Empty))
                return BadRequest($"ID không hợp lệ trong danh sách {operation}");

            var result = await _vaccineTypeService.DeleteVaccineTypesAsync(request.Ids, request.IsPermanent);
            return HandleBatchOperationResult(result);
        }

        /// <summary>
        /// Khôi phục loại vaccine (hỗ trợ cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreVaccineTypes([FromBody] RestoreVaccineTypesRequest request)
        {
            // Inline validation
            if (request?.Ids == null || !request.Ids.Any())
                return BadRequest("Danh sách ID không được rỗng");

            if (request.Ids.Count > 100)
                return BadRequest("Không thể khôi phục quá 100 mục cùng lúc");

            if (request.Ids.Any(id => id == Guid.Empty))
                return BadRequest("ID không hợp lệ trong danh sách");

            var result = await _vaccineTypeService.RestoreVaccineTypesAsync(request.Ids);
            return HandleBatchOperationResult(result);
        }

        #endregion

        #region Soft Delete Operations

        /// <summary>
        /// Lấy danh sách loại vaccine đã bị xóa mềm
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedVaccineTypes(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _vaccineTypeService.GetSoftDeletedVaccineTypesAsync(
                pageNumber, pageSize, searchTerm);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Business Operations

        /// <summary>
        /// Lấy danh sách loại vaccine đang hoạt động
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveVaccineTypes()
        {
            var result = await _vaccineTypeService.GetActiveVaccineTypesAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Thay đổi trạng thái hoạt động của loại vaccine
        /// </summary>
        [HttpPatch("{id:guid}/toggle-status")]
        public async Task<IActionResult> ToggleVaccineTypeStatus(Guid id)
        {
            var result = await _vaccineTypeService.ToggleVaccineTypeStatusAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Private Helper Methods

        // ✅ FIX: Định nghĩa abstract class
        public abstract class BatchIdsRequest
        {
            public List<Guid> Ids { get; set; } = new();
        }

        private static void ValidateBatchRequest(BatchIdsRequest request, int maxItems, string operation)
        {
            if (request?.Ids == null || !request.Ids.Any())
                throw new ArgumentException("Danh sách ID không được rỗng");

            if (request.Ids.Count > maxItems)
                throw new ArgumentException($"Không thể {operation} quá {maxItems} loại vaccine cùng lúc");

            if (request.Ids.Any(id => id == Guid.Empty))
                throw new ArgumentException("Danh sách chứa ID không hợp lệ");
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