using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

// OCR motoru tarafından çıkarılan verilerin veritabanına kaydedilmesi ve kullanıcı tarafından manuel düzeltilmesi (Audit Log destekli) süreçlerini yöneten servis sözleşmesi.
public interface IExtractedFieldService
{
    Task SaveResultsAsync(int pdfDocumentId, List<ExtractedResultDto> results);
    Task<List<ExtractedResultDto>> GetByPdfIdAsync(int pdfDocumentId);

    /// <summary>
    /// Tek bir alanın kullanıcı tarafından düzeltilmiş değerini kaydeder ve Audit Log yazar.
    /// </summary>
    Task<bool> UpdateFieldAsync(int fieldId, UpdateExtractedFieldDto dto, int userId);

    /// <summary>
    /// Bir PDF için birden fazla alan düzeltmesini toplu kaydeder.
    /// </summary>
    Task SaveCorrectionsAsync(int pdfDocumentId, List<UpdateExtractedFieldDto> corrections, int userId);
}
