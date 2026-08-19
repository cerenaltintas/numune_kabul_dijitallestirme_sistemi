using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

public interface IPdfService
{
    Task<PdfUploadResultDto> UploadPdfAsync(Stream fileStream, string fileName, int? institutionId, int? templateId);
    Task<PdfDocumentDto?> GetPdfByIdAsync(int id);
    Task<IEnumerable<PdfDocumentDto>> GetAllPdfsAsync();
    Task<IEnumerable<PdfDocumentDto>> GetPdfsByInstitutionAsync(int institutionId);
    Task<IEnumerable<PdfDocumentDto>> GetPdfsByStatusAsync(string status);
    Task<PaginatedResult<PdfDocumentDto>> GetPdfsPaginatedAsync(int page, int pageSize);
    Task<PaginatedResult<PdfDocumentDto>> GetPdfsPaginatedByInstitutionAsync(int page, int pageSize, int institutionId);
    Task<(Stream FileStream, string FileName)?> DownloadPdfAsync(int id);
    Task<bool> DeletePdfAsync(int id);
    /// <summary>
    /// OCR motorundan dönen metni, güven skorunu ve kelimeleri (koordinatlarıyla) veritabanındaki OcrResults tablosuna kaydeder.
    /// </summary>
    Task SaveOcrResultAsync(int pdfId, OcrEngineResultDto result);

    Task UpdateConfidenceScoreAsync(int pdfId, decimal score);

    /// <summary>
    /// Veritabanından o PDF'e ait kaydedilmiş OCR metnini, güven skorunu ve kelime koordinatlarını getirir.
    /// </summary>
    Task<OcrEngineResultDto?> GetSavedOcrDataAsync(int pdfId);

    /// <summary>
    /// PDF belgesine kurum ve şablon ataması yapar.
    /// </summary>
    Task UpdateTemplateAsync(int pdfId, int institutionId, int templateId);
}
