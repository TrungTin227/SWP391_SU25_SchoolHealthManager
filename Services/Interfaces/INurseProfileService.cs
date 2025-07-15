using DTOs.NurseProfile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface INurseProfileService
    {
        Task<ApiResult<List<NurseProfileRespondDTOs>>> GetAllNurseAsync();
    }
}
