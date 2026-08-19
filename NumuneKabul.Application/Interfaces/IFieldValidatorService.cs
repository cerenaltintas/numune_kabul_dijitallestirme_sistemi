using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

public interface IFieldValidatorService
{
    /// <summary>
    /// Çıkarılan OCR sonucunu, TemplateField üzerindeki Validasyon kurallarına göre doğrular.
    /// Kuralı ihlal eden veriler için güven (Confidence) skorunu düşürür.
    /// </summary>
    void ValidateAndAdjustConfidence(ExtractedResultDto result, TemplateFieldDto field);
    /// <summary>
    /// Çıkarılan sonucun şablon kurallarına (ValidationRegex, DataType vb.) uyup uymadığını kontrol eder.
    /// Geçersiz OCR okumalarını elemek için kullanılır.
    /// </summary>
    bool IsValid(ExtractedResultDto result, TemplateFieldDto field);
}
