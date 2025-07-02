using DTOs.GlobalDTO.Respond;
using DTOs.SessionStudentDTOs.Requests;
using DTOs.SessionStudentDTOs.Responds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class SessionStudentService : BaseService<SessionStudentService, Guid>, ISessionStudentService
    {
        private readonly ISchoolHealthEmailService _schoolHealthEmailService;
        private readonly ILogger<SessionStudentService> _logger;
        public SessionStudentService(
            IGenericRepository<SessionStudentService, Guid> repository,
            ICurrentUserService currentUserService, IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            ISchoolHealthEmailService schoolHealthEmailService,
            ILogger<SessionStudentService> logger) : base(repository, currentUserService, unitOfWork, currentTime)
        {
            _schoolHealthEmailService = schoolHealthEmailService;
            _logger = logger;
        }
        #region Parent Accept Vaccine
        public async Task<ApiResult<List<ParentAcptVaccineResult>>> ParentAcptVaccineAsync(ParentAcptVaccine request)
        {
            var resultList = new List<ParentAcptVaccineResult>();
            var now = _currentTime.GetVietnamTime();

            try
            {
                if (request.StudentIds == null || !request.StudentIds.Any())
                {
                    return ApiResult<List<ParentAcptVaccineResult>>.Failure(new Exception("Danh sách học sinh không được để trống!"));
                }

                var schedule = await _unitOfWork.VaccinationScheduleRepository.GetByIdAsync(request.VaccinationScheduleId);
                if (schedule == null)
                {
                    return ApiResult<List<ParentAcptVaccineResult>>.Failure(new Exception("Không tìm thấy lịch tiêm chủng!"));
                }

                if (!Enum.IsDefined(typeof(ParentConsentStatus), request.ConsentStatus))
                {
                    return ApiResult<List<ParentAcptVaccineResult>>.Failure(new Exception("Giá trị ConsentStatus không hợp lệ!"));
                }

                foreach (var studentId in request.StudentIds)
                {
                    var result = new ParentAcptVaccineResult { StudentId = studentId };

                    try
                    {
                        var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);
                        if (student == null)
                        {
                            result.IsSuccess = false;
                            result.Message = "Không tìm thấy học sinh.";
                            resultList.Add(result);
                            continue;
                        }

                        if (_currentUserService.GetUserId() != student.ParentUserId)
                        {
                            result.IsSuccess = false;
                            result.Message = "Bạn không có quyền cập nhật cho học sinh này.";
                            resultList.Add(result);
                            continue;
                        }

                        var sessionStudent = await _unitOfWork.SessionStudentRepository
                            .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.VaccinationScheduleId == request.VaccinationScheduleId);

                        if (sessionStudent == null)
                        {
                            result.IsSuccess = false;
                            result.Message = "Không tìm thấy thông tin session của học sinh.";
                            resultList.Add(result);
                            continue;
                        }

                        if (sessionStudent.ConsentDeadline.HasValue && now > sessionStudent.ConsentDeadline.Value)
                        {
                            result.IsSuccess = false;
                            result.Message = "Đã hết hạn đồng ý tiêm chủng.";
                            resultList.Add(result);
                            sessionStudent.ConsentStatus = ParentConsentStatus.Expired;
                            continue;
                        }

                        // Update sessionStudent
                        sessionStudent.ConsentStatus = request.ConsentStatus;
                        sessionStudent.ParentSignedAt = now;
                        sessionStudent.ParentNotes = request.ParentNote;
                        sessionStudent.ParentSignature = request.ParentSignature;
                        sessionStudent.UpdatedAt = now;

                        var userId = _currentUserService.GetUserId();
                        if (userId.HasValue)
                        {
                            sessionStudent.UpdatedBy = userId.Value;
                        }

                        await _unitOfWork.SessionStudentRepository.UpdateAsync(sessionStudent);

                        result.IsSuccess = true;
                        result.Message = "Cập nhật thành công.";
                    }
                    catch (Exception exInner)
                    {
                        result.IsSuccess = false;
                        result.Message = $"Lỗi khi xử lý học sinh: {exInner.Message}";
                    }

                    resultList.Add(result);
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<List<ParentAcptVaccineResult>>.Success(resultList, "Xử lý cập nhật đồng ý tiêm chủng hoàn tất.");
            }
            catch (Exception ex)
            {
                return ApiResult<List<ParentAcptVaccineResult>>.Failure(new Exception($"Lỗi hệ thống: {ex.Message}"));
            }
        }
        #endregion  
        #region Get Session Students
        public async Task<ApiResult<List<SessionStudentRespondDTO>>> GetSessionStudentsWithOptionalFilterAsync(GetSessionStudentsRequest request)
        {
            try
            {
                IQueryable<SessionStudent> query = _unitOfWork.SessionStudentRepository.GetQueryable();

                // Lọc theo StudentId nếu có
                if (request.StudentId.HasValue)
                {
                    query = query.Where(ss => ss.StudentId == request.StudentId.Value);
                }
                // Nếu không có StudentId nhưng có ParentId thì lọc theo con của phụ huynh đó
                else if (request.ParentId.HasValue)
                {
                    var studentIds = await _unitOfWork.StudentRepository
                                    .GetQueryable()
                                    .Where(s => s.ParentUserId == request.ParentId.Value)
                                    .Select(s => s.Id)
                                    .ToListAsync();


                    if (!studentIds.Any())
                    {
                        return ApiResult<List<SessionStudentRespondDTO>>.Success(new List<SessionStudentRespondDTO>(), "Không tìm thấy học sinh nào.");
                    }

                    query = query.Where(ss => studentIds.Contains(ss.StudentId));
                }

                // Optional filter thêm theo VaccinationScheduleId nếu có
                if (request.VaccinationScheduleId.HasValue)
                {
                    query = query.Where(ss => ss.VaccinationScheduleId == request.VaccinationScheduleId.Value);
                }

                var sessionStudents = await query.ToListAsync();

                // Map sang DTO
                var result = sessionStudents
                            .Select(ss => ss.ToRespondDTO())
                            .ToList();

                if (!result.Any())
                {
                    return ApiResult<List<SessionStudentRespondDTO>>.Success(new List<SessionStudentRespondDTO>(), "Không tìm thấy SessionStudent nào phù hợp với điều kiện lọc.");
                }

                return ApiResult<List<SessionStudentRespondDTO>>.Success(result,"Lấy danh sách SessionStudent thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<SessionStudentRespondDTO>>.Failure(new Exception($"Lỗi khi lấy danh sách SessionStudent: {ex.Message}"));
            }
        }

        #endregion
        #region update check-in time and status

        public async Task<ApiResult<List<SessionStudentRespondDTO>>> UpdateCheckinTimeById(UpdateSessionStudentCheckInRequest request)
        {
            try
            {
                var now = _currentTime.GetVietnamTime();
                var userId = _currentUserService.GetUserId();

                if (request.SessionStudentId == null || !request.SessionStudentId.Any())
                {
                    return ApiResult<List<SessionStudentRespondDTO>>.Failure(new Exception("Danh sách SessionStudentId không được để trống!"));
                }

                var sessionStudents = await _unitOfWork.SessionStudentRepository
                                    .GetQueryable()
                                    .Where(ss => request.SessionStudentId.Contains(ss.Id))
                                    .ToListAsync();

                if (!sessionStudents.Any())
                {
                    return ApiResult<List<SessionStudentRespondDTO>>.Failure(new Exception("Không tìm thấy session student nào khớp với ID đã cung cấp!"));
                }

                var resultList = new List<SessionStudentRespondDTO>();

                foreach (var ss in sessionStudents)
                {
                    ss.CheckInTime = now;
                    if (!string.IsNullOrWhiteSpace(request.Note))
                        ss.Notes = request.Note;
                    ss.Status = SessionStatus.Present;

                    ss.UpdatedAt = now;
                    if (userId.HasValue)
                        ss.UpdatedBy = userId.Value;

                    await _unitOfWork.SessionStudentRepository.UpdateAsync(ss);

                    resultList.Add(ss.ToRespondDTO());
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<List<SessionStudentRespondDTO>>.Success(resultList, "Cập nhật check-in thành công!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<SessionStudentRespondDTO>>.Failure(new Exception($"Lỗi khi cập nhật check-in: {ex.Message}"));
            }
        }

        public async Task<ApiResult<List<SessionStudentRespondDTO>>> UpdateSessionStudentStatus(UpdateSessionStatus request)
        {
            try
            {
                var now = _currentTime.GetVietnamTime();
                var userId = _currentUserService.GetUserId();

                if (request.SessionStudentIds == null || !request.SessionStudentIds.Any())
                {
                    return ApiResult<List<SessionStudentRespondDTO>>.Failure(new Exception("Danh sách SessionStudentIds không được để trống!"));
                }

                var sessionStudents = await _unitOfWork.SessionStudentRepository
                    .GetQueryable()
                    .Where(ss => request.SessionStudentIds.Contains(ss.Id))
                    .ToListAsync();

                if (!sessionStudents.Any())
                {
                    return ApiResult<List<SessionStudentRespondDTO>>.Failure(new Exception("Không tìm thấy session student nào với ID đã cung cấp!"));
                }

                var updatedList = new List<SessionStudentRespondDTO>();

                foreach (var ss in sessionStudents)
                {

                    ss.Status = request.Status;
                    ss.UpdatedAt = now;

                    if (userId.HasValue)
                        ss.UpdatedBy = userId.Value;

                    await _unitOfWork.SessionStudentRepository.UpdateAsync(ss);
                    updatedList.Add(ss.ToRespondDTO()); // mapping đã tạo
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<List<SessionStudentRespondDTO>>.Success(updatedList, "Cập nhật trạng thái Session thành công!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<SessionStudentRespondDTO>>.Failure(new Exception($"Lỗi khi cập nhật trạng thái Session: {ex.Message}"));
            }
        }

#endregion
        #region Restore Session Student
        public async Task<RestoreResponseDTO> RestoreSessionStudentAsync(Guid id, Guid? userId)
        {
            try
            {
                var restored = await _repository.RestoreAsync(id, userId);
                return new RestoreResponseDTO
                {
                    Id = id,
                    IsSuccess = restored,
                    Message = restored ? "Khôi phục học sinh trong chiến dịch thành công" : "Không thể khôi phục học sinh trong chiến dịch"
                };
            }
            catch (Exception ex)
            {
                return new RestoreResponseDTO { Id = id, IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<List<RestoreResponseDTO>> RestoreSessionStudentRangeAsync(List<Guid> ids, Guid? userId)
        {
            var results = new List<RestoreResponseDTO>();
            foreach (var id in ids)
            {
                results.Add(await RestoreSessionStudentAsync(id, userId));
            }
            return results;
        }

        #endregion
        #region Private Methods
        private async Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(List<Guid> studentIds, string vaccineName, VaccinationSchedule schedule)
        {
            try
            {
                // 1. Lấy toàn bộ học sinh liên quan
                var students = await _unitOfWork.StudentRepository
                    .GetByIdsAsync(studentIds);

                // 2. Lấy toàn bộ parentId
                var parentIds = students
                    .Select(s => s.ParentUserId)
                    .Distinct()
                    .ToList();

                // 3. Lấy thông tin phụ huynh theo batch
                var parents = await _unitOfWork.UserRepository
                    .GetByIdsAsync(parentIds);

                // 4. Map phụ huynh để tra nhanh
                var parentDict = parents.ToDictionary(p => p.Id);

                // 5. Gửi email theo từng học sinh
                foreach (var student in students)
                {
                    if (parentDict.TryGetValue(student.ParentUserId, out var parent))
                    {
                        await _schoolHealthEmailService.SendVaccinationConsentRequestAsync(parent.Email,
                            student.FullName,
                            vaccineName,
                            schedule.ScheduledAt);
                    }
                }
                // 4. Sau khi gửi xong email, update trạng thái trong SessionStudent
                var consentDeadline = schedule.ScheduledAt.AddDays(-1); // ví dụ hạn chót ký là trước 1 ngày
                await MarkParentNotificationStatusAsync(
                    studentIds,
                    schedule.Id,
                    consentDeadline);
                return ApiResult<bool>.Success(true, "Vaccination notification emails sent successfully.");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(new Exception($"Error processing batch student notifications: {ex.Message}"));
            }
        }
        private async Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(Guid studentId, string VaccineName, VaccinationSchedule schedule)
        {
            return await SendVaccinationNotificationEmailToParents(new List<Guid> { studentId }, VaccineName, schedule);
        }
        private async Task<ApiResult<bool>> MarkParentNotificationStatusAsync(List<Guid> studentIds, Guid vaccinationScheduleId, DateTime consentDeadline)
        {
            try
            {
                if (studentIds == null || !studentIds.Any())
                {
                    return ApiResult<bool>.Failure(new Exception("Danh sách học sinh không được để trống!"));
                }
                if (vaccinationScheduleId == Guid.Empty)
                {
                    return ApiResult<bool>.Failure(new Exception("VaccinationScheduleId không được để trống!"));
                }
                if (consentDeadline == default)
                {
                    return ApiResult<bool>.Failure(new Exception("ConsentDeadline không được để trống!"));
                }
                // 1. Lấy toàn bộ SessionStudent liên quan đến danh sách học sinh và lịch tiêm chủng
                var sessionStudents = await _unitOfWork.SessionStudentRepository
                    .GetQueryable()
                    .Where(ss => studentIds.Contains(ss.StudentId) &&
                                 ss.VaccinationScheduleId == vaccinationScheduleId)
                    .ToListAsync();

                var now = _currentTime.GetVietnamTime();

                foreach (var ss in sessionStudents)
                {
                    ss.ParentNotifiedAt = now;
                    ss.ConsentStatus = ParentConsentStatus.Sent;
                    ss.ConsentDeadline = consentDeadline;
                }

                await _unitOfWork.SessionStudentRepository.UpdateRangeAsync(sessionStudents);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, "Cập nhật thời gian thông báo cho phụ huynh tiêm chủng thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(new Exception($"Lỗi khi cập nhật trạng thái thông báo phụ huynh: {ex.Message}"));
            }
        }
        #endregion  

    }
}
