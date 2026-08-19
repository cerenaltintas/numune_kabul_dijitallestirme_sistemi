using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

public interface IExtractionStrategy
{
    /// <summary>
    /// Bu stratejinin belirtilen alan için çalışıp çalışamayacağını belirler.
    /// </summary>
    bool CanExecute(TemplateFieldDto field);

    /// <summary>
    /// OCR metni üzerinden ilgili alanı çıkarır.
    /// allFields parametresi, keyword sınırlama (boundary detection) için diğer alan tanımlarını sağlar.
    /// </summary>
    ExtractedResultDto Extract(OcrEngineResultDto ocrResult, TemplateFieldDto field, List<TemplateFieldDto> allFields);
}
