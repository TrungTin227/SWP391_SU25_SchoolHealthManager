﻿using DTOs.GlobalDTO.Respond;
using DTOs.SessionStudentDTOs.Requests;
using DTOs.SessionStudentDTOs.Responds;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ISessionStudentService
    {
        Task <ApiResult<List<ParentAcptVaccineResult>>> ParentAcptVaccineAsync(ParentAcptVaccine request);
        Task<ApiResult<List<SessionStudentRespondDTO>>> GetSessionStudentsWithOptionalFilterAsync(GetSessionStudentsRequest request);
        Task<ApiResult<List<SessionStudentRespondDTO>>> UpdateCheckinTimeById(UpdateSessionStudentCheckInRequest request);   
        Task<ApiResult<List<SessionStudentRespondDTO>>> UpdateSessionStudentStatus(UpdateSessionStatus request);
        Task<RestoreResponseDTO> RestoreSessionStudentAsync(Guid id, Guid? userId);
        Task<List<RestoreResponseDTO>> RestoreSessionStudentRangeAsync(List<Guid> ids, Guid? userId);
        Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(List<Guid> studentId, string VaccineName, VaccinationSchedule schedule);
        Task<ApiResult<bool>> SendVaccinationNotificationEmailToParents(Guid studentId, string VaccineName, VaccinationSchedule schedule);

    }
}
