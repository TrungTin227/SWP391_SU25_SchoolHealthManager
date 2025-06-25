using DTOs.SessionStudentDTOs.Requests;
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

        public async Task<ApiResult<bool>> ParentAcptVaccineAsync(ParentAcptVaccine request)
        {
            try
            {
                _logger.LogInformation("Starting ParentAcptVaccineAsync for StudentId: {StudentId}, VaccinationScheduleId: {VaccinationScheduleId}", request.StudentId, request.VaccinationScheduleId);
                // 1. Lấy thông tin SessionStudent theo Id

                var sessionStudent = await _unitOfWork.SessionStudentRepository.FirstOrDefaultAsync(ss => ss.StudentId == request.StudentId && ss.VaccinationScheduleId == request.VaccinationScheduleId);

                if (sessionStudent == null)
                {
                    return ApiResult<bool>.Failure(new Exception("Session student not found."));
                }
                // 2. Cập nhật trạng thái đồng ý của phụ huynh
                sessionStudent.ConsentStatus = request.ConsentStatus;
                sessionStudent.ParentSignedAt = DateTime.UtcNow;
                sessionStudent.ParentNotes = request.ParentNote;
                sessionStudent.ParentSignature = request.ParentSignature;
                sessionStudent.UpdatedAt = DateTime.UtcNow;
                var userId = _currentUserService.GetUserId();
                if (userId.HasValue)
                {
                    sessionStudent.UpdatedBy = userId.Value;
                }
                else
                {
                    _logger.LogWarning("UserId is null in ParentAcptVaccineAsync");
                    // Có thể return lỗi hoặc xử lý theo logic riêng tuỳ yêu cầu
                }

                // 3. Lưu thay đổi
                await _unitOfWork.SessionStudentRepository.UpdateAsync(sessionStudent);
                await _unitOfWork.SaveChangesAsync();
                return ApiResult<bool>.Success(true, "Parent consent status updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(new Exception($"Lỗi khi gán giá trị cho session học sinh!!: {ex.Message}"));
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

            var now = DateTime.UtcNow;

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
