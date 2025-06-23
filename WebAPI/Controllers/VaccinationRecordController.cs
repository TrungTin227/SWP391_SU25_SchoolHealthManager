using DTOs.VaccinationRecordDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaccinationRecordController : ControllerBase
    {
        private readonly IVaccinationRecordService _vaccinationRecordService;

        public VaccinationRecordController(IVaccinationRecordService vaccinationRecordService)
        {
            _vaccinationRecordService = vaccinationRecordService;
        }

        [HttpPost("create-vaccination-record")]
        public async Task<IActionResult> Create([FromBody] CreateVaccinationRecordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _vaccinationRecordService.CreateAsync(request);
            return Ok(result);
        }

        [HttpGet("get-all-vaccination-record")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vaccinationRecordService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("get-vaccination-record-by-id")]
        public async Task<IActionResult> GetById([FromQuery] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { Message = "vui lòng nhập Id" });

            var result = await _vaccinationRecordService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { Message = "Không tìm thấy thông tin tiêm chủng!" });

            return Ok(result);
        }

        [HttpPut("update-vaccination-record")]
        public async Task<IActionResult> Update([FromQuery] Guid id, [FromBody] UpdateVaccinationRecordRequest request)
        {
            if (!ModelState.IsValid || id == Guid.Empty)
                return BadRequest(ModelState);

            var result = await _vaccinationRecordService.UpdateAsync(id, request);
            if (result == null)
                return NotFound(new { Message = "Không tìm thấy thông tin tiêm chủng!" });

            return Ok(result);
        }

        [HttpDelete("delete-vaccination-record")]
        public async Task<IActionResult> Delete([FromQuery] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { Message = "Vui lòng nhập Id!" });

            var result = await _vaccinationRecordService.DeleteAsync(id);
            if (!result)
                return NotFound(new { Message = "Không tìm thấy thông tin tiêm chủng!" });

            return Ok(new { Message = "Xóa thành công!" });
        }
    }
}
