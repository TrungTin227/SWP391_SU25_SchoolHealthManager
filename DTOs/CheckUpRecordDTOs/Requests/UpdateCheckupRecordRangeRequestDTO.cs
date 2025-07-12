using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.CheckUpRecordDTOs.Requests
{
    namespace DTOs.CheckUpRecordDTOs.Requests
    {
        public class UpdateCheckupRecordRangeRequestDTO
        {
            [Required, MinLength(1)]
            public List<UpdateCheckupRecordRequestDTO> Records { get; set; } = [];
        }
    }

}
