using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class FileAttachment : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string ReferenceType { get; set; }
        public Guid ReferenceId { get; set; }
        [MaxLength(500)]
        public string FilePath { get; set; }
        [MaxLength(50)]
        public string FileType { get; set; }
    }
}
