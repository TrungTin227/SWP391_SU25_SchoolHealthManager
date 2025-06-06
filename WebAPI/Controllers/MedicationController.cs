using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu xác thực cho tất cả endpoints
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _medicationService;
        private readonly ILogger<MedicationController> _logger;

        public MedicationController(
            IMedicationService medicationService,
            ILogger<MedicationController> logger)
        {
            _medicationService = medicationService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách thuốc theo phân trang với khả năng tìm kiếm và lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedications(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] MedicationCategory? category = null)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("Số trang phải lớn hơn 0");
                }

                var result = await _medicationService.GetMedicationsAsync(
                    pageNumber, pageSize, searchTerm, category);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedications");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết thuốc theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicationById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationService.GetMedicationByIdAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationById for ID: {MedicationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Tạo mới một thuốc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMedication([FromBody] CreateMedicationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _medicationService.CreateMedicationAsync(request);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateMedication");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Cập nhật thông tin thuốc
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedication(Guid id, [FromBody] UpdateMedicationRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _medicationService.UpdateMedicationAsync(id, request);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateMedication for ID: {MedicationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Xóa thuốc (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteMedication(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationService.DeleteMedicationAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteMedication for ID: {MedicationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Khôi phục thuốc đã bị xóa mềm
        /// </summary>
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> RestoreMedication(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationService.RestoreMedicationAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in RestoreMedication for ID: {MedicationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Xóa vĩnh viễn thuốc
        /// </summary>
        [HttpDelete("{id:guid}/permanent")]
        public async Task<IActionResult> PermanentDeleteMedication(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationService.PermanentDeleteMedicationAsync(id);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PermanentDeleteMedication for ID: {MedicationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy danh sách thuốc đã bị xóa mềm
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetSoftDeletedMedications(
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

                var result = await _medicationService.GetSoftDeletedMedicationsAsync(
                    pageNumber, pageSize, searchTerm);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetSoftDeletedMedications");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Dọn dẹp các thuốc đã bị xóa mềm quá thời hạn
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupExpiredMedications(
            [FromQuery][Range(1, 365)] int daysToExpire = 30)
        {
            try
            {
                var result = await _medicationService.CleanupExpiredMedicationsAsync(daysToExpire);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CleanupExpiredMedications");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy danh sách thuốc theo danh mục
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetMedicationsByCategory(MedicationCategory category)
        {
            try
            {
                if (!Enum.IsDefined(typeof(MedicationCategory), category))
                {
                    return BadRequest("Danh mục thuốc không hợp lệ");
                }

                var result = await _medicationService.GetMedicationsByCategoryAsync(category);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationsByCategory for category: {Category}", category);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy danh sách thuốc đang hoạt động
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMedications()
        {
            try
            {
                var result = await _medicationService.GetActiveMedicationsAsync();

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetActiveMedications");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy danh sách các danh mục thuốc có sẵn
        /// </summary>
        [HttpGet("categories")]
        public IActionResult GetMedicationCategories()
        {
            try
            {
                var categories = Enum.GetValues<MedicationCategory>()
                    .Select(c => new { Value = (int)c, Name = c.ToString() })
                    .ToList();

                var result = new { IsSuccess = true, Data = categories, Message = "Lấy danh sách danh mục thành công" };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationCategories");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy danh sách các trạng thái thuốc có sẵn
        /// </summary>
        [HttpGet("statuses")]
        public IActionResult GetMedicationStatuses()
        {
            try
            {
                var statuses = Enum.GetValues<MedicationStatus>()
                    .Select(s => new { Value = (int)s, Name = s.ToString() })
                    .ToList();

                var result = new { IsSuccess = true, Data = statuses, Message = "Lấy danh sách trạng thái thành công" };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationStatuses");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }
    }
}