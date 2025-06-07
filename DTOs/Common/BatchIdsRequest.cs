using System.ComponentModel.DataAnnotations;

namespace DTOs.Common
{
    public class BatchIdsRequest
    {
        [Required(ErrorMessage = "Danh sách ID không được rỗng")]
        [MinLength(1, ErrorMessage = "Phải có tối thiểu 1 ID")]
        public List<Guid> Ids { get; set; } = new();
    }
}
