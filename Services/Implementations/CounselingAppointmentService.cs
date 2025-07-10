using DTOs.CounselingAppointmentDTOs.Requests;
using DTOs.CounselingAppointmentDTOs.Responds;
using DTOs.GlobalDTO.Respond;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class CounselingAppointmentService : BaseService<CounselingAppointment, Guid>, ICounselingAppointmentService
    {
        private readonly ILogger<CounselingAppointmentService> _logger;

        public CounselingAppointmentService(
            IGenericRepository<CounselingAppointment, Guid> repository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            ILogger<CounselingAppointmentService> logger) 
            : base(repository, 
                  currentUserService, 
                  unitOfWork, 
                  currentTime)
        {
            _logger = logger;
        }

       
        #region Create, Update Counseling Appointment

        public async Task<ApiResult<CounselingAppointmentRespondDTO>> CreateCounselingAppointmentAsync(CreateCounselingAppointmentRequestDTO request)
        {
            try
            {
                if (!IsWithinWorkingHours(request.AppointmentDate))
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Lịch tư vấn phải nằm trong giờ làm việc của y tá từ 7h-18h"));

                if (request == null)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new ArgumentNullException(nameof(request)));

                if (request.Duration <= 0)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Thời lượng tư vấn không hợp lệ."));

                // Check tồn tại trước
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId);
                if (student == null)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Không tìm thấy học sinh."));

                var parent = await _unitOfWork.ParentRepository.GetParentByUserIdAsync(request.ParentId);
                if (parent == null)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Không tìm thấy phụ huynh."));

                var staff = await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(request.StaffUserId);
                if (staff == null)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Không tìm thấy nhân viên."));

                var isNurse = await _unitOfWork.NurseProfileRepository
                    .AnyAsync(n => n.UserId == request.StaffUserId);
                if (!isNurse)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Nhân viên không phải là y tá."));

                // Check lịch trùng
                var startTime = request.AppointmentDate;
                var endTime = startTime.AddMinutes(request.Duration);

                var existingAppointments = await _unitOfWork.CounselingAppointmentRepository
                    .GetQueryable()
                    .Where(a => a.StaffUserId == request.StaffUserId
                             && !a.IsDeleted
                             && a.AppointmentDate.Date == startTime.Date)
                    .ToListAsync();

                bool isOverlapping = existingAppointments.Any(a =>
                    startTime < a.AppointmentDate.AddMinutes(a.Duration) &&
                    endTime > a.AppointmentDate);

                if (isOverlapping)
                {
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(
                        new Exception("Nhân viên đã có lịch tư vấn trong khung giờ này."));
                }

                // Tạo lịch tư vấn
                var appointment = new CounselingAppointment
                {
                    StudentId = request.StudentId,
                    ParentId = request.ParentId,
                    StaffUserId = request.StaffUserId,
                    AppointmentDate = request.AppointmentDate,
                    Duration = request.Duration,
                    Purpose = request.Purpose,
                    CheckupRecordId = request.CheckupRecordId,
                    VaccinationRecordId = request.VaccinationRecordId,
                    Status = ScheduleStatus.Pending,
                    IsDeleted = false,
                    CreatedAt = _currentTime.GetVietnamTime(),
                    CreatedBy = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001")
                };

                //await CreateAsync(appointment);
                //await _unitOfWork.SaveChangesAsync();
                var respond = CounselingAppointmentMappings.MapToCounselingAppointmentResponseDTO(appointment);
                return ApiResult<CounselingAppointmentRespondDTO>.Success(respond, "Tạo lịch tư vấn thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch tư vấn");
                return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Tạo lịch tư vấn thất bại: " + ex.Message));
            }
        }

        public async Task<ApiResult<CounselingAppointmentRespondDTO>> UpdateAppointmentAsync(UpdateCounselingAppointmentRequestDTO request)
        {
            try
            {
                // Lấy lịch tư vấn
                var appointment = await _unitOfWork.CounselingAppointmentRepository.GetByIdAsync(request.Id);

                if (appointment == null || appointment.IsDeleted)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Không tìm thấy lịch tư vấn."));

                if (request.AppointmentDate != null && !IsWithinWorkingHours(request.AppointmentDate.Value))
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Lịch tư vấn phải nằm trong giờ làm việc của y tá từ 7h-18h"));

                if (appointment.Status != ScheduleStatus.Pending)
                    return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Lịch tư vấn chỉ có thể cập nhật khi trong trạng thái chưa giải quyết, không thể cập nhật."));

                // 👉 Update Student nếu có
                if (request.StudentId != null)
                {
                    var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId.Value);
                    if (student == null)
                        return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Không tìm thấy học sinh."));
                    appointment.StudentId = request.StudentId.Value;
                }

                // 👉 Update Parent nếu có
                if (request.ParentId != null)
                {
                    var parent = await _unitOfWork.ParentRepository.GetParentByUserIdAsync(request.ParentId.Value);
                    if (parent == null)
                        return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Không tìm thấy phụ huynh."));
                    appointment.ParentId = request.ParentId.Value;
                }

                // 👉 Update Staff nếu có
                if (request.StaffUserId != null)
                {
                    var staff = await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(request.StaffUserId.Value);
                    if (staff == null)
                        return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Không tìm thấy nhân viên."));

                    var isNurse = await _unitOfWork.NurseProfileRepository.AnyAsync(n => n.UserId == request.StaffUserId.Value);
                    if (!isNurse)
                        return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Nhân viên không phải là y tá."));

                    appointment.StaffUserId = request.StaffUserId.Value;
                }

                // 👉 Update ngày hẹn nếu có
                if (request.AppointmentDate != null)
                {
                    appointment.AppointmentDate = request.AppointmentDate.Value;
                }

                // 👉 Update duration nếu có
                if (request.Duration != null)
                {
                    if (request.Duration <= 0)
                        return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Thời lượng tư vấn không hợp lệ."));
                    appointment.Duration = request.Duration.Value;
                }

                // 👉 Các trường khác
                if (!string.IsNullOrWhiteSpace(request.Purpose))
                    appointment.Purpose = request.Purpose;

                if (request.CheckupRecordId != null)
                    appointment.CheckupRecordId = request.CheckupRecordId;

                if (request.VaccinationRecordId != null)
                    appointment.VaccinationRecordId = request.VaccinationRecordId;

                // Update thông tin audit
                appointment.UpdatedAt = _currentTime.GetVietnamTime();
                appointment.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001");

                await UpdateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                // Map kết quả
                var response = CounselingAppointmentMappings.MapToCounselingAppointmentResponseDTO(appointment);

                return ApiResult<CounselingAppointmentRespondDTO>.Success(response, "Cập nhật lịch tư vấn thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lịch tư vấn");
                return ApiResult<CounselingAppointmentRespondDTO>.Failure(new Exception("Cập nhật lịch tư vấn thất bại: " + ex.Message));
            }
        }
#endregion
        #region Get Counseling Appointment

        public async Task<ApiResult<CounselingAppointmentRespondDTO?>> GetByIdAsync(Guid id)
        {
            try
            {
                var appointment = await _unitOfWork.CounselingAppointmentRepository.GetByIdAsync(id);

                if (appointment == null || appointment.IsDeleted)
                    return ApiResult<CounselingAppointmentRespondDTO?>.Failure(new Exception("Không tìm thấy lịch tư vấn."));

                var response = CounselingAppointmentMappings.MapToCounselingAppointmentResponseDTO(appointment);
                return ApiResult<CounselingAppointmentRespondDTO?>.Success(response, "Lấy lịch tư vấn theo Id thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch tư vấn theo Id");
                return ApiResult<CounselingAppointmentRespondDTO?>.Failure(new Exception("Lấy lịch tư vấn thất bại: " + ex.Message));
            }
        }

        public async Task<ApiResult<List<CounselingAppointmentRespondDTO?>>> GetAllByStaffIdAsync(Guid id)
        {
            try
            {
                // Check nhân viên tồn tại
                var staff = await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(id);
                if (staff == null)
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(new Exception("Không tìm thấy nhân viên."));

                var isNurse = await _unitOfWork.NurseProfileRepository.AnyAsync(n => n.UserId == id);
                if (!isNurse)
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(new Exception("Nhân viên không phải là y tá."));

                var appointments = await _unitOfWork.CounselingAppointmentRepository
                    .GetQueryable()
                    .Where(a => a.StaffUserId == id && !a.IsDeleted)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();

                if (appointments == null || !appointments.Any())
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(new Exception("Không tìm thấy lịch tư vấn."));

                var responseList = appointments
                    .Select(CounselingAppointmentMappings.MapToCounselingAppointmentResponseDTO)
                    .ToList();

                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Success(responseList, "Lấy danh sách lịch tư vấn theo staff thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tư vấn theo StaffId");
                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(new Exception("Lấy lịch tư vấn thất bại: " + ex.Message));
            }
        }


        public async Task<ApiResult<List<CounselingAppointmentRespondDTO?>>> GetAllPendingByStaffIdAsync(Guid id)
        {
            try
            {
                // ✅ Check nhân viên có tồn tại không
                var staff = await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(id);
                if (staff == null)
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                        new Exception("Không tìm thấy nhân viên."));

                // ✅ Check có phải y tá không
                var isNurse = await _unitOfWork.NurseProfileRepository.AnyAsync(n => n.UserId == id);
                if (!isNurse)
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                        new Exception("Nhân viên không phải là y tá."));

                // ✅ Lấy danh sách lịch tư vấn Pending
                var pendingAppointments = await _unitOfWork.CounselingAppointmentRepository
                    .GetQueryable()
                    .Where(a => a.StaffUserId == id
                                && !a.IsDeleted
                                && a.Status == ScheduleStatus.Pending)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();

                if (!pendingAppointments.Any())
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                        new Exception("Không có lịch tư vấn nào đang chờ xử lý."));

                // ✅ Map sang DTO
                var responseList = pendingAppointments
                    .Select(CounselingAppointmentMappings.MapToCounselingAppointmentResponseDTO)
                    .ToList();

                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Success(responseList, "Lấy danh sách lịch tư vấn Pending thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tư vấn Pending theo StaffId");
                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                    new Exception("Lấy lịch tư vấn thất bại: " + ex.Message));
            }
        }


        public async Task<ApiResult<List<CounselingAppointmentRespondDTO?>>> GetAllByStudentCodeAsync(string studentCode)
        {
            try
            {
                // 1. Tìm học sinh theo studentCode
                var student = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.StudentCode == studentCode && !s.IsDeleted);

                if (student == null)
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                        new Exception("Không tìm thấy học sinh với mã đã cho."));

                // 2. Lấy danh sách lịch tư vấn
                var appointments = await _unitOfWork.CounselingAppointmentRepository
                    .GetQueryable()
                    .Where(a => a.StudentId == student.Id && !a.IsDeleted)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();

                if (appointments == null || !appointments.Any())
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                        new Exception("Học sinh này chưa có lịch tư vấn nào."));

                // 3. Map sang DTO
                var responseList = appointments
                    .Select(CounselingAppointmentMappings.MapToCounselingAppointmentResponseDTO)
                    .ToList();

                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Success(responseList, "Lấy danh sách lịch tư vấn theo mã học sinh thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tư vấn theo StudentCode");
                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                    new Exception("Lấy lịch tư vấn thất bại: " + ex.Message));
            }
        }

        public async Task<ApiResult<List<CounselingAppointmentRespondDTO?>>> GetAllByStudentIdAsync(Guid studentId)
        {
            try
            {
                // 1. Tìm học sinh theo studentCode
                var student = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

                if (student == null)
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                        new Exception("Không tìm thấy học sinh với id đã cho."));

                // 2. Lấy danh sách lịch tư vấn
                var appointments = await _unitOfWork.CounselingAppointmentRepository
                    .GetQueryable()
                    .Where(a => a.StudentId == student.Id && !a.IsDeleted)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();

                if (appointments == null || !appointments.Any())
                    return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                        new Exception("Học sinh này chưa có lịch tư vấn nào."));

                // 3. Map sang DTO
                var responseList = appointments
                    .Select(CounselingAppointmentMappings.MapToCounselingAppointmentResponseDTO)
                    .ToList();

                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Success(responseList, "Lấy danh sách lịch tư vấn theo id học sinh thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tư vấn theo StudentId");
                return ApiResult<List<CounselingAppointmentRespondDTO?>>.Failure(
                    new Exception("Lấy lịch tư vấn thất bại: " + ex.Message));
            }
        }

        #endregion
        #region Accept, Reject, Add Note and Recommend
        public async Task<ApiResult<AddNoteAndRecommendRequestDTO>> AddNoteAndRecommend(AddNoteAndRecommendRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    return ApiResult<AddNoteAndRecommendRequestDTO>.Failure(
                        new ArgumentNullException(nameof(request), "Dữ liệu gửi lên không được để trống."));
                }

                if (request.CounselingAppointmentId == Guid.Empty)
                {
                    return ApiResult<AddNoteAndRecommendRequestDTO>.Failure(
                        new ArgumentException("ID lịch tư vấn không hợp lệ."));
                }

                var appointment = await _unitOfWork.CounselingAppointmentRepository.GetByIdAsync(request.CounselingAppointmentId);
                if (appointment == null || appointment.IsDeleted)
                {
                    return ApiResult<AddNoteAndRecommendRequestDTO>.Failure(
                        new Exception("Không tìm thấy lịch tư vấn."));
                }

                // Cập nhật ghi chú và lời khuyên
                appointment.Notes = request.Notes;
                appointment.Recommendations = request.Recommendations;
                appointment.Status = ScheduleStatus.Completed; // Cập nhật trạng thái nếu cần
                appointment.UpdatedAt = _currentTime.GetVietnamTime();
                appointment.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001");

                await _unitOfWork.CounselingAppointmentRepository.UpdateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<AddNoteAndRecommendRequestDTO>.Success(request, "Cập nhật ghi chú và lời khuyên thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm ghi chú và lời khuyên cho tư vấn");
                return ApiResult<AddNoteAndRecommendRequestDTO>.Failure(
                    new Exception("Tạo ghi chú và lời khuyên thất bại: " + ex.Message));
            }
        }
        public async Task<ApiResult<bool>> AcceptAppointmentAsync(Guid appointmentId)
        {
            try
            {

                if (appointmentId == Guid.Empty)
                {
                    return ApiResult<bool>.Failure(
                        new ArgumentException("ID lịch tư vấn không hợp lệ."));
                }

                var appointment = await _unitOfWork.CounselingAppointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null || appointment.IsDeleted)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Không tìm thấy lịch tư vấn."));
                }
                if (appointment.Status != ScheduleStatus.Pending)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Lịch tư vấn không ở trạng thái chờ."));
                }
                // Cập nhật trạng thái
                appointment.Status = ScheduleStatus.InProgress;
                appointment.UpdatedAt = _currentTime.GetVietnamTime();
                appointment.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
                await _unitOfWork.CounselingAppointmentRepository.UpdateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();
                return ApiResult<bool>.Success(true, "Bắt đầu lịch tư vấn thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bắt đầu lịch tư vấn");
                return ApiResult<bool>.Failure(new Exception("Bắt đầu lịch tư vấn thất bại: " + ex.Message));
            }
        }

        public async Task<ApiResult<bool>> RejectAppointmentAsync(Guid appointmentId)
        {
            try
            {

                if (appointmentId == Guid.Empty)
                {
                    return ApiResult<bool>.Failure(
                        new ArgumentException("ID lịch tư vấn không hợp lệ."));
                }

                var appointment = await _unitOfWork.CounselingAppointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null || appointment.IsDeleted)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Không tìm thấy lịch tư vấn."));
                }
                if (appointment.Status != ScheduleStatus.Pending)
                {
                    return ApiResult<bool>.Failure(
                        new Exception("Lịch tư vấn không ở trạng thái chờ."));
                }
                // Cập nhật trạng thái
                appointment.Status = ScheduleStatus.Cancelled;
                appointment.UpdatedAt = _currentTime.GetVietnamTime();
                appointment.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
                await _unitOfWork.CounselingAppointmentRepository.UpdateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();
                return ApiResult<bool>.Success(true, "Hủy lịch tư vấn thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy lịch tư vấn");
                return ApiResult<bool>.Failure(new Exception("Hủy lịch tư vấn thất bại: " + ex.Message));
            }
        }
#endregion
        #region Restore, Delete Counseling Appointment
        public async Task<RestoreResponseDTO> RestoreCounselingAppointmentAsync(Guid id, Guid? userId)
        {
            try
            {
                var restored = await _repository.RestoreAsync(id, userId);
                return new RestoreResponseDTO
                {
                    Id = id,
                    IsSuccess = restored,
                    Message = restored ? "Khôi phục cuộc hẹn tư vấn thành công" : "Không thể khôi phục cuộc hẹn tư vấn"
                };
            }
            catch (Exception ex)
            {
                return new RestoreResponseDTO { Id = id, IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<List<RestoreResponseDTO>> RestoreCounselingAppointmentRangeAsync(List<Guid> ids, Guid? userId)
        {
            var results = new List<RestoreResponseDTO>();
            foreach (var id in ids)
            {
                results.Add(await RestoreCounselingAppointmentAsync(id, userId));
            }
            return results;
        }

        public async Task<ApiResult<bool>> SoftDeleteAsync(Guid id)
        {
            try
            {
                var appointment = await _unitOfWork.CounselingAppointmentRepository.GetByIdAsync(id);
                if (appointment == null || appointment.IsDeleted)
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy lịch tư vấn để xóa."));

                appointment.IsDeleted = true;
                appointment.DeletedAt = _currentTime.GetVietnamTime();
                appointment.DeletedBy = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001");

                await _unitOfWork.CounselingAppointmentRepository.UpdateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, "Xóa mềm lịch tư vấn thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm lịch tư vấn");
                return ApiResult<bool>.Failure(new Exception("Xóa mềm lịch tư vấn thất bại: " + ex.Message));
            }
        }


        public async Task<ApiResult<bool>> SoftDeleteRangeAsync(List<Guid> idList)
        {
            try
            {
                if (idList == null || !idList.Any())
                    return ApiResult<bool>.Failure(new Exception("Danh sách ID trống."));

                var appointments = await _unitOfWork.CounselingAppointmentRepository
                    .GetQueryable()
                    .Where(a => idList.Contains(a.Id) && !a.IsDeleted)
                    .ToListAsync();

                if (!appointments.Any())
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy lịch tư vấn nào để xóa."));

                var now = _currentTime.GetVietnamTime();
                var userId = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001");

                foreach (var appointment in appointments)
                {
                    appointment.IsDeleted = true;
                    appointment.DeletedAt = now;
                    appointment.DeletedBy = userId;

                    await _unitOfWork.CounselingAppointmentRepository.UpdateAsync(appointment);
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, "Xóa mềm danh sách lịch tư vấn thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm danh sách lịch tư vấn");
                return ApiResult<bool>.Failure(new Exception("Xóa mềm danh sách lịch tư vấn thất bại: " + ex.Message));
            }
        }
        #endregion
    }
}
