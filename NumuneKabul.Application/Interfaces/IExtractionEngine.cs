using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

public interface IExtractionEngine
{
    List<ExtractedResultDto> ExtractFields(OcrEngineResultDto ocrResult, List<TemplateFieldDto> templateFields);
}
