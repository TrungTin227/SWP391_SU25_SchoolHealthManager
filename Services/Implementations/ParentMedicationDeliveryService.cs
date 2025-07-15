using BusinessObjects.Common;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Respond;
using Repositories;
using Repositories.Interfaces;
using Services.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class ParentMedicationDeliveryService : BaseService<ParentMedicationDelivery, Guid>, IParentMedicationDeliveryService
    {
        private readonly SchoolHealthManagerDbContext _dbContext;

        // Constructor sử dụng base service để khởi tạo repository, current user service và unit of work
        public ParentMedicationDeliveryService(
            IGenericRepository<ParentMedicationDelivery, Guid> parentMedicationDeliveryRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            SchoolHealthManagerDbContext dbContext,
            ICurrentTime currentTime
            )
        :
            base(parentMedicationDeliveryRepository, currentUserService, unitOfWork, currentTime)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResult<bool>> UpdateStatus(Guid parentMedicationDeliveryid, StatusMedicationDelivery status)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                if (parentMedicationDeliveryid == Guid.Empty)
                    return ApiResult<bool>.Failure(new Exception("ID đơn giao thuốc phụ huynh không hợp lệ."));


                if (currentUserId == null)
                    return ApiResult<bool>.Failure(new Exception("Đăng nhập trước khi thực hiện thao tác này!"));
                if (await _unitOfWork.NurseProfileRepository.GetNurseByUserIdAsync(currentUserId.Value)==null)
                        return ApiResult<bool>.Failure(new Exception("ID người dùng không phải là nurse!!."));

                if (!Enum.IsDefined(typeof(StatusMedicationDelivery), status))
                    return ApiResult<bool>.Failure(new Exception("Trạng thái không hợp lệ."));

                var parentMedicationDelivery = await _unitOfWork.ParentMedicationDeliveryRepository
                    .GetByIdAsync(parentMedicationDeliveryid);

                if (parentMedicationDelivery == null)
                    return ApiResult<bool>.Failure(new Exception($"Không tìm thấy đơn giao thuốc phụ huynh với ID: {parentMedicationDeliveryid}"));

                if ((int)status < (int)parentMedicationDelivery.Status)
                    return ApiResult<bool>.Failure(new Exception($"Không thể cập nhật trạng thái từ {parentMedicationDelivery.Status} về {status}."));

                parentMedicationDelivery.Status = status;
                parentMedicationDelivery.UpdatedAt = _currentTime.GetVietnamTime();
                parentMedicationDelivery.UpdatedBy = currentUserId.Value;
                parentMedicationDelivery.ReceivedBy = currentUserId.Value;

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, "Cập nhật trạng thái giao thuốc phụ huynh thành công!");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(new Exception($"Cập nhật trạng thái thất bại. Lỗi: {ex.Message}"));
            }
        }


        public async Task<ApiResult<CreateParentMedicationDeliveryRequestDTO>> CreateAsync(CreateParentMedicationDeliveryRequestDTO request)
        {
            try
            {
                if (IsWithinWorkingHours(request.DeliveredAt) == false)
                    return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không thể tạo đơn thuốc phụ huynh giao ngoài giờ làm việc."));

                if (await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId) == null)
                    return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy học sinh với ID: " + request.StudentId));

                if (await _unitOfWork.ParentRepository.GetParentByUserIdAsync(request.ParentId) == null)
                    return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy phụ huynh với ID: " + request.ParentId));

                //await _unitOfWork.ParentMedicationDeliveryRepository.CreateParentMedicationDeliveryRequestDTO(request); // gọi base service để tạo entity
                await CreateAsync(ParentMedicationDeliveryMappings.ToParentMedicationDelivery(request)); // map DTO thành entity và gọi base service để tạo entity

                return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Success(request, "Tạo đơn thuốc phụ huynh giao thành công!!!"); // ví dụ
            }
            catch (Exception ex)
            {
                return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Tạo đơn thuốc phụ huynh giao thất bại với exception: " + ex.Message));
            }
        }

        public async Task<ApiResult<List<GetParentMedicationDeliveryRespondDTO>>> GetAllAsync()
        {
            try
            {
                var list =await _unitOfWork.ParentMedicationDeliveryRepository.GetAllParentMedicationDeliveryDTO();
                if (list == null || !list.Any())
                {
                    return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Failure(new Exception("Không có đơn thuốc phụ huynh giao nào được tìm thấy."));
                }
                return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Success(list, "Lấy danh sách đơn thuốc phụ huynh giao thành công.");
            }
            catch (Exception ex)
            {
                return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Failure(new Exception("Lấy danh sách đơn thuốc phụ huynh giao thất bại với exception: " + ex.Message));
            }
        }

        public async Task<ApiResult<List<GetParentMedicationDeliveryRespondDTO>>> GetAllParentMedicationDeliveryByParentIdAsync(Guid parentId)
        {
            try
            {
                if (await _unitOfWork.ParentRepository.GetParentByUserIdAsync(parentId) == null)
                    return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Failure(new Exception("Không tìm thấy phụ huynh với ID : " + parentId));

                var list = await _unitOfWork.ParentMedicationDeliveryRepository.GetAllParentMedicationDeliveryByParentIdDTO(parentId);
                if (list == null || !list.Any())
                    return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Failure(new Exception("Không có đơn thuốc phụ huynh giao nào được tìm thấy."));
                return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Success(list, "Lấy danh sách đơn thuốc phụ huynh giao thành công.");
            }
            catch (Exception ex)
            {
                return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Failure(new Exception("Lấy danh sách đơn thuốc phụ huynh giao thất bại với exception: " + ex.Message));
            }
        }

        public async Task<ApiResult<List<GetParentMedicationDeliveryRespondDTO>>> GetAllPendingAsync()
        {
            try
            {
                var list = await _unitOfWork.ParentMedicationDeliveryRepository.GetAllAsync(x => x.Status == StatusMedicationDelivery.Pending);
                if (list == null || !list.Any())
                {
                    return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Failure(new Exception("Không có đơn thuốc phụ huynh giao nào được tìm thấy."));
                }

                var responds = list.Select(x => ParentMedicationDeliveryMappings.ToResponds(x)).ToList();

                return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Success(responds, "Lấy danh sách đơn thuốc phụ huynh giao thành công.");
            }
            catch (Exception ex)
            {
                return ApiResult<List<GetParentMedicationDeliveryRespondDTO>>.Failure(new Exception("Lấy danh sách đơn thuốc phụ huynh giao thất bại với exception: " + ex.Message));
            }
        }

        public async Task<ApiResult<GetParentMedicationDeliveryRespondDTO?>> GetByIdAsync(Guid id)
        {
            var results = await _unitOfWork.ParentMedicationDeliveryRepository.GetParentMedicationDeliveryByIdDTO(id);
            if (results == null)
                return ApiResult<GetParentMedicationDeliveryRespondDTO?>.Failure(new Exception("Không tìm thấy thuốc phụ huynh giao với Id " + id));
            return ApiResult<GetParentMedicationDeliveryRespondDTO?>.Success(results, "Lấy thuốc của phụ huynh thành công!!");
        }

        public async Task<ApiResult<UpdateParentMedicationDeliveryRequestDTO>> UpdateAsync(UpdateParentMedicationDeliveryRequestDTO request)
        {
            try
            {
                var update = await _unitOfWork.ParentMedicationDeliveryRepository.GetByIdAsync(request.ParentMedicationDeliveryId);
                if (update == null)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy đơn thuốc phụ huynh giao với ID: " + request.ParentMedicationDeliveryId));
                if (request.DeliveredAt != null && IsWithinWorkingHours(request.DeliveredAt.Value) == false)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không thể cập nhật đơn thuốc phụ huynh giao ngoài giờ làm việc."));

                if (request.StudentId.HasValue && await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId.Value) == null)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy học sinh với ID: " + request.StudentId));

                if (request.ReceivedBy.HasValue && await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(request.ReceivedBy.Value) == null)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy người nhận với ID: " + request.ReceivedBy));

                if (request.ParentId.HasValue && await _unitOfWork.ParentRepository.GetParentByUserIdAsync(request.ParentId.Value) == null)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy phụ huynh với ID: " + request.ParentId));
                ParentMedicationDeliveryMappings.ToUpdateParentMedicationDelivery(request, update);
                var currentUserId = _currentUserService.GetUserId();
                update.UpdatedAt = _currentTime.GetVietnamTime();
                update.UpdatedBy = currentUserId ?? Guid.Empty;
                // map DTO thành entity
                //await base.UpdateAsync(entity); 
                await _dbContext.SaveChangesAsync(); // Tự động update vào DB

                return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Success(request, "Cập nhật đơn thuốc phụ huynh giao thành công!!!"); // ví dụ

            }
            catch (Exception ex)
            {
                return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Cập nhật đơn thuốc phụ huynh giao thất bại với exception: " + ex.Message));
            }
        }
    }
}
