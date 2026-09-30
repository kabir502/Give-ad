using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NGO_website.Services.Interfaces;

namespace NGO_website.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveFileAsync(
            IFormFile? file, 
            string subFolder, 
            string[]? allowedExtensions = null, 
            long maxSizeBytes = 5 * 1024 * 1024)
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, "No file provided.");
            }

            if (file.Length > maxSizeBytes)
            {
                return (false, null, $"File size exceeds the allowed limit of {maxSizeBytes / (1024 * 1024)} MB.");
            }

            allowedExtensions ??= new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return (false, null, $"Invalid file type '{extension}'. Allowed types: {string.Join(", ", allowedExtensions)}");
            }

            try
            {
                var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", subFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var relativePath = $"/uploads/{subFolder}/{uniqueFileName}";
                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving uploaded file.");
                return (false, null, "An error occurred while saving the file on the server.");
            }
        }

        public void DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath)) return;

            try
            {
                var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var cleanRelative = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(webRootPath, cleanRelative);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file at {Path}", relativeFilePath);
            }
        }
    }
}
