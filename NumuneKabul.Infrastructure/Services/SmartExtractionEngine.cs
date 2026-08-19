using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services;

/// <summary>
/// Şablon kurallarını uygulayan akıllı veri çıkarma motoru.
/// </summary>
public class SmartExtractionEngine : IExtractionEngine
{
    private readonly IEnumerable<IExtractionStrategy> _strategies;
    private readonly ILogger<SmartExtractionEngine> _logger;
    private readonly IFieldValidatorService _validatorService;

    public SmartExtractionEngine(IEnumerable<IExtractionStrategy> strategies, ILogger<SmartExtractionEngine> logger, IFieldValidatorService validatorService)
    {
        _strategies = strategies;
        _logger = logger;
        _validatorService = validatorService;
    }

    public List<ExtractedResultDto> ExtractFields(OcrEngineResultDto ocrResult, List<TemplateFieldDto> templateFields)
    {
        var results = new List<ExtractedResultDto>();

        foreach (var field in templateFields)
        {
            ExtractedResultDto? bestResult = null;
            
            // Stratejiler üzerinden sırayla geç (Regex, ardından Keyword)
            foreach (var strategy in _strategies)
            {
                if (strategy.CanExecute(field))
                {
                    _logger.LogDebug("Alan '{FieldName}' için '{StrategyName}' stratejisi deneniyor.", field.FieldName, strategy.GetType().Name);
                    
                    var result = strategy.Extract(ocrResult, field, templateFields);
                    
                    if (result.RawValue != null)
                    {
                        // Doğrulama servisinden geçir. Eğer geçersizse (örn: çöp OCR sonucu) kabul etme.
                        if (_validatorService.IsValid(result, field))
                        {
                            bestResult = result;
                            _logger.LogInformation("Field {Field} için '{StrategyName}' stratejisi BAŞARILI oldu. Bulunan değer: '{Value}'", field.FieldName, strategy.GetType().Name, result.RawValue);
                            break; // Geçerli değer bulunduysa diğer stratejileri denemeye gerek yok
                        }
                        else
                        {
                            _logger.LogWarning("Field {Field} için '{StrategyName}' stratejisi değer buldu ('{Value}') ANCAK geçerlilik kontrolünü (Validation) geçemedi. Fallback devam ediyor...", field.FieldName, strategy.GetType().Name, result.RawValue);
                        }
                    }
                }
            }

            // Hiçbir strateji değer bulamadıysa boş result ekle
            if (bestResult == null)
            {
                _logger.LogWarning("Field {Field} için hiçbir strateji değer bulamadı! Tamamen başarısız (Tüm fallback'ler tükendi).", field.FieldName);
                bestResult = new ExtractedResultDto
                {
                    FieldName = field.FieldName,
                    PageNo = 1,
                    Confidence = 0,
                    RawValue = null,
                    X = field.X ?? 0,
                    Y = field.Y ?? 0,
                    Width = field.Width ?? 0,
                    Height = field.Height ?? 0
                };
            }

            // Doğrulama servisinden geçir ve güven skorunu gerekirse ayarla
            _validatorService.ValidateAndAdjustConfidence(bestResult, field);

            _logger.LogInformation("Field {Field} için son kullanıcıya gönderilen nihai değer: '{Value}' (Güven Skoru: {Confidence})", field.FieldName, bestResult.RawValue ?? "[NULL]", bestResult.Confidence);
            results.Add(bestResult);
        }

        return results;
    }
}
