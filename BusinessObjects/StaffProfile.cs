using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class StaffProfile : BaseEntity
    {
        [Key]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public User User { get; set; }

        [MaxLength(100)]
        public string Position { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }
    }
}
