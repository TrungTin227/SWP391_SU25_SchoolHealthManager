using DTOs.CounselingAppointmentDTOs.Requests;
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

        public async Task<ApiResult<CreateCounselingAppointmentRequestDTO>> CreateCounselingAppointmentAsync(CreateCounselingAppointmentRequestDTO request)
        {
            try
            {
                if (request == null)
                    return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(new ArgumentNullException(nameof(request)));

                if (request.Duration <= 0)
                    return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(new Exception("Thời lượng tư vấn không hợp lệ."));

                // Check tồn tại trước
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId);
                if (student == null)
                    return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(new Exception("Không tìm thấy học sinh."));

                var parent = await _unitOfWork.ParentRepository.GetParentByUserIdAsync(request.ParentId);
                if (parent == null)
                    return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(new Exception("Không tìm thấy phụ huynh."));

                var staff = await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(request.StaffUserId);
                if (staff == null)
                    return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(new Exception("Không tìm thấy nhân viên."));

                var isNurse = await _unitOfWork.NurseProfileRepository
                    .AnyAsync(n => n.UserId == request.StaffUserId);
                if (!isNurse)
                    return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(new Exception("Nhân viên không phải là y tá."));

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
                    return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(
                        new Exception("Nhân viên đã có lịch tư vấn trong khung giờ này."));
                }

                // Tạo lịch tư vấn
                var appointment = new CounselingAppointment
                {
                    Id = Guid.NewGuid(),
                    StudentId = request.StudentId,
                    ParentId = request.ParentId,
                    StaffUserId = request.StaffUserId,
                    AppointmentDate = request.AppointmentDate,
                    Duration = request.Duration,
                    Purpose = request.Purpose,
                    //Notes = request.Notes,
                    //Recommendations = request.Recommendations,
                    CheckupRecordId = request.CheckupRecordId,
                    VaccinationRecordId = request.VaccinationRecordId,
                    Status = ScheduleStatus.Pending,
                    IsDeleted = false,
                    CreatedAt = _currentTime.GetVietnamTime(),
                    CreatedBy = _currentUserService.GetUserId() ?? Guid.Parse("00000000-0000-0000-0000-000000000001")
                };

                await CreateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<CreateCounselingAppointmentRequestDTO>.Success(request, "Tạo lịch tư vấn thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch tư vấn");
                return ApiResult<CreateCounselingAppointmentRequestDTO>.Failure(new Exception("Tạo lịch tư vấn thất bại: " + ex.Message));
            }
        }


    }
}
