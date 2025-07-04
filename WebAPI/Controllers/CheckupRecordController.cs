﻿using DTOs.CheckUpRecordDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/checkup-records")]
    public class CheckupRecordController : Controller
    {
        private readonly ICheckupRecordService _checkupRecordService;

        public CheckupRecordController(ICheckupRecordService checkupRecordService)
        {
            _checkupRecordService = checkupRecordService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckupRecord([FromBody] CreateCheckupRecordRequestDTO request)
        {
            if (request == null)
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });

            var result = await _checkupRecordService.CreateCheckupRecordAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetAllByStaffId(Guid staffId)
        {
            var result = await _checkupRecordService.GetAllByStaffIdAsync(staffId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("student/{studentCode}")]
        public async Task<IActionResult> GetAllByStudentCode(string studentCode)
        {
            var result = await _checkupRecordService.GetAllByStudentCodeAsync(studentCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _checkupRecordService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var result = await _checkupRecordService.SoftDeleteAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("soft-delete")]
        public async Task<IActionResult> SoftDeleteRange([FromBody] List<Guid> ids)
        {
            var result = await _checkupRecordService.SoftDeleteRangeAsync(ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
