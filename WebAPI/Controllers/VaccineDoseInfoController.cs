using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class VaccineDoseInfoController : ControllerBase
    {
        private readonly IVaccineDoseInfoService _vaccineDoseInfoService;

        public VaccineDoseInfoController(
            IVaccineDoseInfoService vaccineDoseInfoService)
        {
            _vaccineDoseInfoService = vaccineDoseInfoService;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy danh sách thông tin liều vaccine với phân trang và bộ lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVaccineDoseInfos(
            [FromQuery] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] Guid? vaccineTypeId = null,
            [FromQuery] int? doseNumber = null)
        {
            var result = await _vaccineDoseInfoService.GetVaccineDoseInfosAsync(
                pageNumber, pageSize, vaccineTypeId, doseNumber);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
     
        /// <summary>
        /// Lấy thông tin chi tiết liều vaccine theo ID
        /// </summary>
        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetVaccineDoseInfoDetailById(Guid id)
        {
            var result = await _vaccineDoseInfoService.GetVaccineDoseInfoDetailByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Tạo mới thông tin liều vaccine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVaccineDoseInfo([FromBody] CreateVaccineDoseInfoRequest request)
        {
            var result = await _vaccineDoseInfoService.CreateVaccineDoseInfoAsync(request);

            return result.IsSuccess
                ? CreatedAtAction(
                    nameof(GetVaccineDoseInfoDetailById),
                    new { id = result.Data?.Id },
                    result)
                : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật thông tin liều vaccine
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVaccineDoseInfo(Guid id, [FromBody] UpdateVaccineDoseInfoRequest request)
        {
            var result = await _vaccineDoseInfoService.UpdateVaccineDoseInfoAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Xóa thông tin liều vaccine (hỗ trợ cả đơn lẻ và hàng loạt)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteVaccineDoseInfos([FromBody] DeleteVaccineDoseInfosRequest request)
        {
            if (request?.Ids == null || !request.Ids.Any())
                return BadRequest("Danh sách ID không được rỗng");

            if (request.Ids.Count > 50)
                return BadRequest("Không thể xóa quá 50 thông tin liều vaccine cùng lúc");

            var result = await _vaccineDoseInfoService.DeleteVaccineDoseInfosAsync(request.Ids, request.IsPermanent);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Business Operations       

        /// <summary>
        /// Lấy thông tin mũi tiêm tiếp theo được khuyến nghị
        /// </summary>
        [HttpGet("next-dose/{vaccineTypeId:guid}/{currentDoseNumber:int}")]
        public async Task<IActionResult> GetNextRecommendedDose(Guid vaccineTypeId, int currentDoseNumber)
        {
            var result = await _vaccineDoseInfoService.GetNextRecommendedDoseAsync(vaccineTypeId, currentDoseNumber);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}