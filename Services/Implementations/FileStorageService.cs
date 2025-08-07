using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Services.Interfaces;
using System.Text;

namespace Services.Implementations
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _basePath;

        public FileStorageService(ILogger<FileStorageService> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _basePath = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads");
            
            // Tạo thư mục uploads nếu chưa tồn tại
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderPath)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty or null");

                // Tạo thư mục nếu chưa tồn tại
                var fullFolderPath = Path.Combine(_basePath, folderPath);
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(fullFolderPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối
                var relativePath = Path.Combine(folderPath, fileName).Replace("\\", "/");
                _logger.LogInformation($"File uploaded successfully: {relativePath}");
                
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<string> UploadImageFromBase64Async(string base64String, string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                    throw new ArgumentException("Base64 string is empty or null");

                // Tạo thư mục nếu chưa tồn tại
                var fullFolderPath = Path.Combine(_basePath, folderPath);
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }

                // Xử lý base64 string
                var base64Data = base64String;
                if (base64String.Contains(","))
                {
                    base64Data = base64String.Substring(base64String.IndexOf(",") + 1);
                }

                // Decode base64
                var imageBytes = Convert.FromBase64String(base64Data);

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(fullFolderPath, fileName);

                // Lưu file
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Trả về đường dẫn tương đối
                var relativePath = Path.Combine(folderPath, fileName).Replace("\\", "/");
                _logger.LogInformation($"Image uploaded successfully: {relativePath}");
                
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image from base64");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"File deleted successfully: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return false;
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (File.Exists(fullPath))
                {
                    return await File.ReadAllBytesAsync(fullPath);
                }
                throw new FileNotFoundException($"File not found: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file");
                throw;
            }
        }

        public Task<string> GetFileUrlAsync(string filePath)
        {
            // Trả về URL tương đối cho file
            var url = $"/uploads/{filePath}";
            return Task.FromResult(url);
        }
    }
} 