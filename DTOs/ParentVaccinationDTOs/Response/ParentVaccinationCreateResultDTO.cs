using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentVaccinationDTOs.Response
{
    public class ParentVaccinationCreateResultDTO
    {
        public bool IsSuccess { get; set; }
        public ParentVaccinationRespondDTO? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}
