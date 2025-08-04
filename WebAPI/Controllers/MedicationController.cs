using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _medicationService;

        public MedicationController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy danh sách thuốc theo phân trang với khả năng tìm kiếm và lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedications(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 100,
            [FromQuery] string? searchTerm = null,
            [FromQuery] MedicationCategory? category = null,
            [FromQuery] bool includeDeleted = false)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn 0");

            var result = await _medicationService.GetMedicationsAsync(
                pageNumber, pageSize, searchTerm, category, includeDeleted);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết thuốc theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicationById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID thuốc không hợp lệ");

            var result = await _medicationService.GetMedicationByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết thuốc kèm thông tin lô
        /// </summary>
        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetMedicationDetailById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID thuốc không hợp lệ");

            var result = await _medicationService.GetMedicationDetailByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Tạo mới một thuốc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMedication([FromBody] CreateMedicationRequest request)
        {
            var result = await _medicationService.CreateMedicationAsync(request);

            return result.IsSuccess ? CreatedAtAction(
                nameof(GetMedicationById),
                new { id = result.Data?.Id },
                result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật thông tin thuốc
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedication(Guid id, [FromBody] UpdateMedicationRequest request)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID thuốc không hợp lệ");

            var result = await _medicationService.UpdateMedicationAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Batch Delete Operations

        /// <summary>
        /// Xóa thuốc (hỗ trợ xóa 1 hoặc nhiều, soft delete hoặc permanent)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteMedications([FromBody] DeleteMedicationsRequest request)
        {
            var result = await _medicationService.DeleteMedicationsAsync(request.Ids, request.IsPermanent);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Khôi phục thuốc đã bị xóa mềm (hỗ trợ 1 hoặc nhiều)
        /// </summary>
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreMedications([FromBody] RestoreMedicationsRequest request)
        {
            var result = await _medicationService.RestoreMedicationsAsync(request.Ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Dọn dẹp các thuốc đã bị xóa mềm quá thời hạn
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupExpiredMedications(
            [FromQuery][Range(1, 365)] int daysToExpire = 30)
        {
            var result = await _medicationService.CleanupExpiredMedicationsAsync(daysToExpire);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Metadata Operations

        /// <summary>
        /// Lấy danh sách các danh mục thuốc có sẵn
        /// </summary>
        [HttpGet("categories")]
        public IActionResult GetMedicationCategories()
        {
            var categories = GetEnumMetadata<MedicationCategory>();
            var result = CreateSuccessResponse(categories, "Lấy danh sách danh mục thành công");

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách các trạng thái thuốc có sẵn
        /// </summary>
        [HttpGet("statuses")]
        public IActionResult GetMedicationStatuses()
        {
            var statuses = GetEnumMetadata<MedicationStatus>();
            var result = CreateSuccessResponse(statuses, "Lấy danh sách trạng thái thành công");

            return Ok(result);
        }

        #endregion

        #region Private Helper Methods

        private static List<object> GetEnumMetadata<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(enumValue => new
                {
                    Value = Convert.ToInt32(enumValue),
                    Name = enumValue.ToString()
                })
                .ToList<object>();
        }

        private static object CreateSuccessResponse(object data, string message)
        {
            return new
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        #endregion
    }
}