using DTOs.SessionStudentDTOs.Requests;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class SessionStudentServce : BaseService<SessionStudentServce, Guid>, ISessionStudentService
    {
        private readonly ISchoolHealthEmailService _schoolHealthEmailService;
        public SessionStudentServce(
            IGenericRepository<SessionStudentServce, Guid> repository,
            ICurrentUserService currentUserService, IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            ISchoolHealthEmailService schoolHealthEmailService) : base(repository, currentUserService, unitOfWork, currentTime)
        {
            _schoolHealthEmailService = schoolHealthEmailService;
        }

        public async Task<ApiResult<bool>> ParentAcptVaccineAsync(Guid sessionStudentId, ParentAcptVaccine request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<bool>> ParentDeclineVaccineAsync(Guid sessionStudentId, ParentAcptVaccine request)
        {
            throw new NotImplementedException();
        }

        //public async Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(List<Guid> studentId, string VaccineName, DateTime schedule)
        //{
        //    //var 
        //    //return await _schoolHealthEmailService.SendVaccinationConsentRequestAsync(studentId);
        //    try {
        //        foreach (Guid studentid in studentId)
        //        {
        //            var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentid);
        //            var parent = await _unitOfWork.UserRepository.GetUserDetailsByIdAsync(student.ParentUserId);
        //            await _schoolHealthEmailService.SendVaccinationConsentRequestAsync(
        //                parent.Email,
        //                student.FullName,
        //                VaccineName,
        //                schedule);
        //        }
        //        return ApiResult<bool>.Success(true, "Vaccination notification email sent successfully to parents.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResult<bool>.Failure(new Exception($"Error processing student IDs: {ex.Message}"));
        //    }
        //}
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

            return ApiResult<bool>.Success(true,"Cập nhật thời gian thông báo cho phụ huynh tiêm chủng thành công!!");
        }

    }
}
