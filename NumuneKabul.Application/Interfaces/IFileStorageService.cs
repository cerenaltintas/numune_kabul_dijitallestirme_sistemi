namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Dosya depolama altyapısına (Local, Bulut vb.) olan bağımlılığı ortadan kaldıran;
/// güvenli dosya okuma, yazma ve silme işlemlerini soyutlayan servis sözleşmesi.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream content, string fileName, string folderName);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream?> GetFileStreamAsync(string filePath);
    string GetFullPath(string relativePath);
}
