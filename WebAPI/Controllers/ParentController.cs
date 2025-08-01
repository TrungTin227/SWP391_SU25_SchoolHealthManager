using DTOs.ParentDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/parents")]
    [Authorize]

    public class ParentController : Controller
    {
        private readonly IParentService _parentService;
        private readonly IParentMedicationDeliveryService _ParentMedicationDeliveryService;
        private readonly IParentMedicationDeliveryDetailService _parentMedicationDeliveryDetailService;

        public ParentController(
            IParentService parentService, 
            IParentMedicationDeliveryService parentMedicationDeliveryService,
            IParentMedicationDeliveryDetailService parentMedicationDeliveryDetailService)
        {
            _parentService = parentService;
            _ParentMedicationDeliveryService = parentMedicationDeliveryService;
            _parentMedicationDeliveryDetailService = parentMedicationDeliveryDetailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterParent([FromBody] UserRegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _parentService.RegisterParentUserAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //[HttpPost("user-register")]
        //public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequestDTO request)
        //{
        //    if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        //        return BadRequest(new { Message = "Email and Password are required" });

        //    var result = await _parentService.RegisterUserAsync(request);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateParent([FromBody] AddParentRequestDTO request)
        //{
        //    if (request == null)
        //        return BadRequest(new { Message = "User ID is required" });

        //    var result = await _parentService.CreateParentAsync(request);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        [HttpPatch("relationship")]
        public async Task<IActionResult> UpdateRelationshipByParentId([FromBody] UpdateRelationshipByParentId request)
        {
            if (request == null || request.ParentId == Guid.Empty)
                return BadRequest(new { Message = "Parent ID is required" });

            var result = await _parentService.UpdateRelationshipByParentIdAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("soft-delete")]
        public async Task<IActionResult> SoftDeleteByParentId([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { Message = "Parent ID is required" });

            var result = await _parentService.SoftDeleteByParentIdListAsync(ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("restore-parents")]
        public async Task<IActionResult> RestoreParents([FromBody] List<Guid> ids)
        {
            var result = await _parentService.RestoreParentRangeAsync(ids, null);
            return Ok(result);
        }

        [HttpPost("medication-deliveries")]
        public async Task<IActionResult> CreateParentMedicationDelivery([FromBody] CreateParentMedicationDeliveryRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ParentMedicationDeliveryService.CreateDeliveryAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //[HttpPut("medication-deliveries")]
        //public async Task<IActionResult> UpdateParentMedicationDelivery([FromBody] UpdateParentMedicationDeliveryRequestDTO request)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    if (request.QuantityDelivered <= 0)
        //        return BadRequest("Số lượng phải lớn hơn 0");

        //    var result = await _ParentMedicationDeliveryService.UpdateAsync(request);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        [HttpGet("medication-deliveries")]
        public async Task<IActionResult> GetAllParentMedicationDelivery()
        {
            var result = await _ParentMedicationDeliveryService.GetAllAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //[HttpGet("medication-deliveries/by-parent/{parentId}")]
        //public async Task<IActionResult> GetAllParentMedicationDeliveryByParentId(Guid parentId)
        //{
        //    if (parentId == Guid.Empty)
        //        return BadRequest(new { Message = "Parent ID is required" });

        //    var result = await _ParentMedicationDeliveryService.GetAllParentMedicationDeliveryByParentIdAsync(parentId);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        //[HttpGet("pending")]
        //public async Task<IActionResult> GetAllPendingParentMedicationDelivery()
        //{
        //    var result = await _ParentMedicationDeliveryService.GetAllPendingAsync();
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}


        [HttpGet("medication-deliveries/{id}")]
        public async Task<IActionResult> GetAllParentMedicationDeliveryById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { Message = "ID is required" });

            var result = await _ParentMedicationDeliveryService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("medication-deliveries/parent/CurrentParent")]
        public async Task<IActionResult> GetAllParentMedicationDeliveryByParentId()
        {
            var result = await _ParentMedicationDeliveryService.GetAllForCurrentParentAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("medication-deliveries/update-status")]
        public async Task<IActionResult> UpdateStatus(Guid parentMedicationDeliveryid, StatusMedicationDelivery status)
        {
            if (parentMedicationDeliveryid == Guid.Empty || !Enum.IsDefined(typeof(StatusMedicationDelivery), status))
                return BadRequest(new { Message = "Parent Medication Delivery ID, Receiver ID, and a valid status are required" });

            var result = await _ParentMedicationDeliveryService.UpdateStatusAsync(parentMedicationDeliveryid, status);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật ReturnedQuantity cho một delivery detail
        /// </summary>
        [HttpPost("medication-deliveries/delivery-details/{deliveryDetailId}/update-returned-quantity")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> UpdateReturnedQuantity(Guid deliveryDetailId)
        {
            if (deliveryDetailId == Guid.Empty)
                return BadRequest(new { Message = "Delivery Detail ID is required" });

            var result = await _parentMedicationDeliveryDetailService.UpdateReturnedQuantityAsync(deliveryDetailId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật ReturnedQuantity cho tất cả delivery details của một delivery
        /// </summary>
        [HttpPost("medication-deliveries/{deliveryId}/update-returned-quantity")]
        //[Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> UpdateReturnedQuantityForDelivery(Guid deliveryId)
        {
            if (deliveryId == Guid.Empty)
                return BadRequest(new { Message = "Delivery ID is required" });

            var result = await _parentMedicationDeliveryDetailService.UpdateReturnedQuantityForDeliveryAsync(deliveryId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


    }
}
