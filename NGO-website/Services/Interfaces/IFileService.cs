using Microsoft.AspNetCore.Http;

namespace NGO_website.Services.Interfaces
{
    public interface IFileService
    {
        Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveFileAsync(IFormFile? file, string subFolder, string[]? allowedExtensions = null, long maxSizeBytes = 5 * 1024 * 1024);
        void DeleteFile(string? relativeFilePath);
    }
}
