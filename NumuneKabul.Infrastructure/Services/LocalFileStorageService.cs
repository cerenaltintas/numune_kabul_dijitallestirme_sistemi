using Microsoft.Extensions.Logging;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream content, string fileName, string folderName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
        var relativePath = Path.Combine(folderName, uniqueFileName);
        var fullPath = GetFullPath(relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await content.CopyToAsync(stream);
        }

        _logger.LogInformation("Dosya yerel diske kaydedildi: {Path}", fullPath);
        return relativePath;
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                _logger.LogInformation("Dosya yerel diskten silindi: {Path}", fullPath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dosya silinirken hata oluştu: {Path}", fullPath);
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(false);
    }

    public Task<Stream?> GetFileStreamAsync(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(fileStream);
    }

    public string GetFullPath(string relativePath)
    {
        var basePath = Path.GetFullPath(Directory.GetCurrentDirectory());
        var fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

        // Oluşan nihai yolun, ana dizin içerisinde kalıp kalmadığını doğrula.
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path Traversal saldırısı tespit edildi. İstenen yol: {Path}", relativePath);
            throw new UnauthorizedAccessException("Güvenlik ihlali: İzin verilmeyen dizine erişim denemesi!");
        }

        return fullPath;
    }
}
