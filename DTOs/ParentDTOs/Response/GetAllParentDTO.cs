using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentDTOs.Response
{
    public class GetAllParentDTO
    {
            public Guid UserId { get; set; }
            public string? Username { get; set; } // nếu có lấy kèm User
            public string Relationship { get; set; } // convert enum thành string dễ nhìn hơn

    }
}
