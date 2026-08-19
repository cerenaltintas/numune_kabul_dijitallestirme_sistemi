namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// PDF işleme ve OCR süreçlerini API katmanından soyutlayarak yöneten merkezi orkestrasyon servisi.
/// </summary>
public interface IDocumentProcessingService
{
    Task<string> ProcessDocumentAsync(int pdfId);
    Task ExtractFieldsAsync(int pdfId, int institutionId, int templateId);
}
