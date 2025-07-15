using DTOs.NurseProfile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers.Mappers
{
    public static class NurseProfileMappings
    {
        public static NurseProfileRespondDTOs ToRespondDTO(NurseProfile nurse)
        {
            return new NurseProfileRespondDTOs
            {
                UserId = nurse.UserId,
                Name = nurse.User?.FullName ?? "N/A", // Check null để tránh crash
                Position = nurse.Position,
                Department = nurse.Department
            };
        }

        public static List<NurseProfileRespondDTOs> ToRespondDTOList(List<NurseProfile> nurses)
        {
            return nurses.Select(ToRespondDTO).ToList();
        }
    }
}
