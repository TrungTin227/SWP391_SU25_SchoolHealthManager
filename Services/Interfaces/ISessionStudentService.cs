using DTOs.SessionStudentDTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ISessionStudentService
    {
        Task <ApiResult<bool>> ParentAcptVaccineAsync(Guid sessionStudentId, ParentAcptVaccine request);
        Task<ApiResult<bool>> ParentDeclineVaccineAsync(Guid sessionStudentId, ParentAcptVaccine request);
        Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(List<Guid> studentId, string VaccineName, VaccinationSchedule schedule );
        Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(Guid studentId, string VaccineName, VaccinationSchedule schedule);


    }
}
