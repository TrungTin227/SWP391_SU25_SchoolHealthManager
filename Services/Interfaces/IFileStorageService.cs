using Microsoft.AspNetCore.Http;

namespace Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderPath);
        Task<string> UploadImageFromBase64Async(string base64String, string folderPath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        Task<string> GetFileUrlAsync(string filePath);
    }
} 