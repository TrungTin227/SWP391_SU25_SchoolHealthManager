using BusinessObjects.Common;
using DTOs.MedicationDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Controllers
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
        /// <param name="pageNumber">Số trang (mặc định: 1)</param>
        /// <param name="pageSize">Kích thước trang (mặc định: 10, tối đa: 100)</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="category">Danh mục thuốc</param>
        /// <returns>Danh sách thuốc phân trang</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
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
        /// <param name="id">ID của thuốc</param>
        /// <returns>Thông tin chi tiết thuốc</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMedicationById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationService.GetMedicationByIdAsync(id);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return NotFound(result);
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
        /// <param name="request">Thông tin thuốc cần tạo</param>
        /// <returns>Thông tin thuốc vừa được tạo</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMedication([FromBody] CreateMedicationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _medicationService.CreateMedicationAsync(request);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(
                        nameof(GetMedicationById),
                        new { id = result.Data.Id },
                        result);
                }

                // Kiểm tra xem có phải lỗi trùng tên không
                if (result.Message?.Contains("đã tồn tại") == true)
                {
                    return Conflict(result);
                }

                return BadRequest(result);
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
        /// <param name="id">ID của thuốc cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Thông tin thuốc sau khi cập nhật</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                // Kiểm tra các loại lỗi khác nhau
                if (result.Message?.Contains("Không tìm thấy") == true)
                {
                    return NotFound(result);
                }

                if (result.Message?.Contains("đã tồn tại") == true)
                {
                    return Conflict(result);
                }

                return BadRequest(result);
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
        /// <param name="id">ID của thuốc cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMedication(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("ID thuốc không hợp lệ");
                }

                var result = await _medicationService.DeleteMedicationAsync(id);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                if (result.Message?.Contains("Không tìm thấy") == true)
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteMedication for ID: {MedicationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }

        /// <summary>
        /// Lấy danh sách thuốc theo danh mục
        /// </summary>
        /// <param name="category">Danh mục thuốc</param>
        /// <returns>Danh sách thuốc theo danh mục</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMedicationsByCategory(MedicationCategory category)
        {
            try
            {
                if (!Enum.IsDefined(typeof(MedicationCategory), category))
                {
                    return BadRequest("Danh mục thuốc không hợp lệ");
                }

                var result = await _medicationService.GetMedicationsByCategoryAsync(category);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
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
        /// <returns>Danh sách thuốc có trạng thái Active</returns>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveMedications()
        {
            try
            {
                var result = await _medicationService.GetActiveMedicationsAsync();

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
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
        /// <returns>Danh sách các enum MedicationCategory</returns>
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMedicationCategories()
        {
            try
            {
                var categories = Enum.GetValues<MedicationCategory>()
                    .Select(c => new { Value = (int)c, Name = c.ToString() })
                    .ToList();

                return Ok(new { IsSuccess = true, Data = categories, Message = "Lấy danh sách danh mục thành công" });
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
        /// <returns>Danh sách các enum MedicationStatus</returns>
        [HttpGet("statuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMedicationStatuses()
        {
            try
            {
                var statuses = Enum.GetValues<MedicationStatus>()
                    .Select(s => new { Value = (int)s, Name = s.ToString() })
                    .ToList();

                return Ok(new { IsSuccess = true, Data = statuses, Message = "Lấy danh sách trạng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMedicationStatuses");
                return StatusCode(500, "Đã xảy ra lỗi không mong muốn");
            }
        }
    }
}