using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Respond;
using Repositories;
using Repositories.Interfaces;
using Services.Commons;

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

        public async Task<ApiResult<bool>> AcptDelivery(Guid parentMedicationDeliveryid, Guid receiverId )
        {
            try
            {
                if (parentMedicationDeliveryid == Guid.Empty || receiverId == Guid.Empty)
                    return ApiResult<bool>.Failure(new Exception("ID đơn giao thuốc phụ huynh hoặc người nhận không hợp lệ."));
                if (await _unitOfWork.UserRepository.GetByIdAsync(receiverId ) == null)
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy người nhận với ID: " + receiverId ));
                var parentMedicationDelivery = await _unitOfWork.ParentMedicationDeliveryRepository.GetByIdAsync(parentMedicationDeliveryid);   
                if (parentMedicationDelivery == null)
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy đơn giao thuốc phụ huynh với ID: " + parentMedicationDeliveryid));
                parentMedicationDelivery.ReceivedBy = receiverId ;
                await _unitOfWork.SaveChangesAsync(); // gọi base service để cập nhật entity
                return ApiResult<bool>.Success(true, "Xác nhận giao thuốc phụ huynh thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(new Exception("Xác nhận giao thuốc phụ huynh thất bại với exception: " + ex.Message));
            }
        }

        public async Task<ApiResult<CreateParentMedicationDeliveryRequestDTO>> CreateAsync(CreateParentMedicationDeliveryRequestDTO request)
        {
            try
            {
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

                if (request.StudentId.HasValue && await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId.Value) == null)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy học sinh với ID: " + request.StudentId));

                if (request.ReceivedBy.HasValue && await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(request.ReceivedBy.Value) == null)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy người nhận với ID: " + request.ReceivedBy));

                if (request.ParentId.HasValue && await _unitOfWork.ParentRepository.GetParentByUserIdAsync(request.ParentId.Value) == null)
                    return ApiResult<UpdateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy phụ huynh với ID: " + request.ParentId));
                ParentMedicationDeliveryMappings.ToUpdateParentMedicationDelivery(request, update);
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
