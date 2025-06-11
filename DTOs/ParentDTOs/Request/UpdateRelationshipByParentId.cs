using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Common;

namespace DTOs.ParentDTOs.Request
{
    public class UpdateRelationshipByParentId
    {
        public Guid ParentId { get; set; } // Id của phụ huynh cần cập nhật
       
        public Relationship Relationship { get; set; } // Mối quan hệ mới (Father/Mother/Guardian...)
    }
}
