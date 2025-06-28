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



        public async Task<ApiResult<bool>> ParentDeclineVaccineAsync(ParentAcptVaccine request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(List<Guid> studentIds, string vaccineName, VaccinationSchedule schedule)
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


        public async Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(Guid studentId, string VaccineName, VaccinationSchedule schedule)
        {
            return await SendVaccinationNotificationEmailToParents(new List<Guid> { studentId }, VaccineName, schedule);
        }

        public async Task<ApiResult<bool>> MarkParentNotificationStatusAsync(
    List<Guid> studentIds,
    Guid vaccinationScheduleId,
    DateTime consentDeadline)
        {
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

    }
}
