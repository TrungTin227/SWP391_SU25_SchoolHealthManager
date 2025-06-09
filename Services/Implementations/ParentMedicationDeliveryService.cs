using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class ParentMedicationDeliveryService : BaseService<ParentMedicationDelivery, Guid>, IParentMedicationDeliveryService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IParentRepository _parentRepository;

        // Constructor sử dụng base service để khởi tạo repository, current user service và unit of work
        public ParentMedicationDeliveryService(
            IGenericRepository<ParentMedicationDelivery, Guid> parentMedicationDeliveryRepository,
            ICurrentUserService currentUserService, 
            IUnitOfWork unitOfWork,
            IStudentRepository studentRepository,
            IUserRepository userRepository,
            IParentRepository parentRepository
            ) 
        : 
            base(parentMedicationDeliveryRepository, currentUserService, unitOfWork)
        {
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _parentRepository = parentRepository;
        }

        public async Task<ApiResult<CreateParentMedicationDeliveryRequestDTO>> CreateAsync(CreateParentMedicationDeliveryRequestDTO request)
        {
            try
            {
                if (await _studentRepository.GetByIdAsync(request.StudentId) == null)
                    return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy học sinh với ID: " + request.StudentId));

                if (await _userRepository.GetUserDetailsByIdAsync(request.ReceivedBy) == null)
                    return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy người nhận với ID: " + request.ReceivedBy));

                if (await _parentRepository.GetParentByUserIdAsync(request.ParentId) == null)
                    return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Không tìm thấy phụ huynh với ID: " + request.ParentId));

                var entity = ParentMedicationDeliveryMappings.ToParentMedicationDelivery(request);  // map DTO thành entity
                var result = await base.CreateAsync(entity); // gọi base service để tạo entity

                return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Success(request, "Tạo đơn thuốc phụ huynh giao thành công!!!"); // ví dụ
            }
            catch (Exception ex)
            {
                return ApiResult<CreateParentMedicationDeliveryRequestDTO>.Failure(new Exception("Tạo đơn thuốc phụ huynh giao thất bại với exception: " + ex));
            }
        }

        public Task<ApiResult<CreateParentMedicationDeliveryRequestDTO>> UpdateAsync(CreateParentMedicationDeliveryRequestDTO request)
        {
            throw new NotImplementedException();
        }
    }
}
