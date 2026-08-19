using System.Drawing.Imaging;
using Microsoft.Extensions.Configuration;
using NumuneKabul.Application.Interfaces;
using PdfiumViewer;

namespace NumuneKabul.Infrastructure.Services;

public class PdfImageService : IPdfImageService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly string _imageUploadFolder;

    public PdfImageService(IFileStorageService fileStorageService, IConfiguration configuration)
    {
        _fileStorageService = fileStorageService;
        _imageUploadFolder = configuration.GetValue<string>("StorageSettings:ImagePath") ?? "uploads/images";
    }

    public async Task<List<string>> ConvertToImagesAsync(string pdfFilePath, string outputDirectory)
    {
        var imagePaths = new List<string>();
        
        // outputDirectory (örn: pdf_5) klasörünü genel image klasörüyle birleştiriyoruz
        string targetFolder = Path.Combine(_imageUploadFolder, outputDirectory);

        // PDF okuma işlemini asenkron olarak arka planda yapıyoruz
        await Task.Run(async () =>
        {
            // Orijinal PDF dosyasını okumak için tam yolunu buluyoruz
            string fullPdfPath = _fileStorageService.GetFullPath(pdfFilePath);
            
            using var document = PdfDocument.Load(fullPdfPath);
            
            // Çok sayfalı PDF'lerin (Zip Bomb) sistemi kilitlemesini engellemek için sınır koyuyoruz.
            const int maxAllowedPages = 50;
            if (document.PageCount > maxAllowedPages)
            {
                throw new InvalidOperationException($"Güvenlik İhlali: PDF belge boyutu çok büyük! Maksimum {maxAllowedPages} sayfaya izin verilmektedir.");
            }

            for (int i = 0; i < document.PageCount; i++)
            {
                var pageSize = document.PageSizes[i];
                
                // OCR doğruluğu ve görüntü kalitesi için sabit 300 DPI kullanıyoruz.
                // Standart PDF boyutu point (1/72 inç) cinsindendir.
                const int dpi = 300;
                int renderWidth = (int)(pageSize.Width * dpi / 72.0);
                int renderHeight = (int)(pageSize.Height * dpi / 72.0);

                using var image = document.Render(i, renderWidth, renderHeight, dpi, dpi, true);
                
                string imageFileName = $"sayfa_{i + 1}.png";
                
                // Resmi geçici olarak hafızaya (MemoryStream) kaydediyoruz
                using var memoryStream = new MemoryStream();
                image.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0; // Başa sar

                // Hafızadaki resmi IFileStorageService ile güvenli şekilde kaydediyoruz
                var savedRelativePath = await _fileStorageService.SaveFileAsync(memoryStream, imageFileName, targetFolder);
                
                imagePaths.Add(savedRelativePath);
            }
        });

        return imagePaths;
    }

    public string GetImageFilePath(int pdfId, int pageNo)
    {
        string folderRelativePath = Path.Combine(_imageUploadFolder, $"pdf_{pdfId}");
        string fullFolderPath = _fileStorageService.GetFullPath(folderRelativePath);
        
        if (Directory.Exists(fullFolderPath))
        {
            var files = Directory.GetFiles(fullFolderPath, $"*_sayfa_{pageNo}.png");
            if (files.Length > 0)
            {
                return files[0];
            }
        }
        
        // Fallback (for older files if any)
        string relativePath = Path.Combine(_imageUploadFolder, $"pdf_{pdfId}", $"sayfa_{pageNo}.png");
        return _fileStorageService.GetFullPath(relativePath);
    }
}
