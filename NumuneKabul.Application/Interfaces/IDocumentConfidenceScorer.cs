using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

public interface IDocumentConfidenceScorer
{
    /// <summary>
    /// OCR motorunun ortalama skoru ve çıkarılan alanların kalitesini kullanarak 
    /// belgenin genel güven skorunu hesaplar.
    /// </summary>
    decimal CalculateDocumentScore(decimal ocrConfidence, IEnumerable<ExtractedResultDto> extractedFields, IEnumerable<TemplateFieldDto> templateFields);
}
