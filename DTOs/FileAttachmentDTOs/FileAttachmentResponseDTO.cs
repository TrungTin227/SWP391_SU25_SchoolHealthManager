using BusinessObjects.Common;

namespace DTOs.FileAttachmentDTOs
{
    public class FileAttachmentResponseDTO
    {
        public Guid Id { get; set; }
        public ReferenceType ReferenceType { get; set; }
        public Guid ReferenceId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public FileType FileType { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
} 