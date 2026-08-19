using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services.ExtractionStrategies;

/// <summary>
/// Zonal OCR modunda OCR motoru tarafından "AlanAdı: Değer" formatında
/// döndürülen yapılandırılmış metinden veriyi çıkaran strateji.
/// </summary>
public class ZonalExtractionStrategy : IExtractionStrategy
{
    private readonly ILogger<ZonalExtractionStrategy> _logger;
    private readonly ICoordinateMapperService _coordinateMapper;

    public ZonalExtractionStrategy(ILogger<ZonalExtractionStrategy> logger, ICoordinateMapperService coordinateMapper)
    {
        _logger = logger;
        _coordinateMapper = coordinateMapper;
    }

    public bool CanExecute(TemplateFieldDto field)
    {
        return !string.IsNullOrWhiteSpace(field.FieldName);
    }

    public ExtractedResultDto Extract(OcrEngineResultDto ocrResult, TemplateFieldDto field, List<TemplateFieldDto> allFields)
    {
        var result = new ExtractedResultDto
        {
            FieldName = field.FieldName,
            PageNo = 1,
            Confidence = 0,
            RawValue = null
        };

        try
        {
            // Sadece ZONAL AREAS bölümünde arama yap (Tam sayfa metninde yanlış eşleşmeleri engelle)
            string zonalText = string.Empty;
            int zonalIndex = ocrResult.Text.IndexOf("--- ZONAL AREAS ---");
            if (zonalIndex >= 0)
            {
                zonalText = ocrResult.Text.Substring(zonalIndex);
            }
            else
            {
                return result; // Zonal alan yoksa eşleşme olamaz
            }
            
            string pattern = $@"^{Regex.Escape(field.FieldName)}:\s*(.+)";
            
            var match = Regex.Match(
                zonalText, 
                pattern, 
                RegexOptions.IgnoreCase | RegexOptions.Multiline,
                TimeSpan.FromSeconds(2)
            );
            
            if (match.Success)
            {
                string value = match.Groups[1].Value.Trim();
                
                // Başındaki olası yapışık ":" veya "-" işaretlerini temizle
                char[] charsToTrim = { ':', '-', '.', ' ', '—', '_', '>' };
                value = value.TrimStart(charsToTrim).Trim();
                
                _logger.LogInformation("Field {Field} Zonal OCR tarafından okunan ham değer: '{Value}'", field.FieldName, value);

                // Eğer "[Okunamadı veya Boş]" ibaresi varsa null bırak
                if (value.Contains("[Okunamadı veya Boş]"))
                {
                    _logger.LogInformation("Field {Field} Zonal başarısız (Okunamadı veya Boş). Diğer stratejilere (Keyword vb.) düşüyor.", field.FieldName);
                    return result;
                }

                result.RawValue = value;

                // Gerçek OCR kelimeleri üzerinden koordinatları bul (Dinamik Highlight)
                int trueIndex = match.Groups[1].Index + zonalIndex;
                var metrics = _coordinateMapper.GetBoundingBoxForMatch(trueIndex, match.Groups[1].Length, ocrResult.Words);
                
                result.Confidence = metrics.AverageConfidence > 0 ? metrics.AverageConfidence : 0.95m;

                if (metrics.Width > 0 && metrics.Height > 0)
                {
                    result.X = metrics.X;
                    result.Y = metrics.Y;
                    result.Width = metrics.Width;
                    result.Height = metrics.Height;
                }
                else if (field.X.HasValue && field.Y.HasValue && field.Width.HasValue && field.Height.HasValue)
                {
                    // Fallback: Eğer eşleşme yapılamadıysa statik şablon kutusunu kullan
                    result.X = field.X.Value;
                    result.Y = field.Y.Value;
                    result.Width = field.Width.Value;
                    result.Height = field.Height.Value;
                }
            }
        }
        catch (RegexMatchTimeoutException ex)
        {
            _logger.LogWarning(ex, "ZonalExtraction timeout for field {FieldName}", field.FieldName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZonalExtraction error for field {FieldName}", field.FieldName);
        }

        return result;
    }
}
