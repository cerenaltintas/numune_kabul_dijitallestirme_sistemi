using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services.ExtractionStrategies;

public class RegexExtractionStrategy : IExtractionStrategy
{
    private readonly ILogger<RegexExtractionStrategy> _logger;
    private readonly ICoordinateMapperService _coordinateMapper;
    private const string PageSeparatorPattern = @"---\s*sayfa_";

    public RegexExtractionStrategy(ILogger<RegexExtractionStrategy> logger, ICoordinateMapperService coordinateMapper)
    {
        _logger = logger;
        _coordinateMapper = coordinateMapper;
    }

    public bool CanExecute(TemplateFieldDto field)
    {
        return !string.IsNullOrWhiteSpace(field.Regex);
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
            var match = Regex.Match(
                ocrResult.Text, 
                field.Regex!, 
                RegexOptions.IgnoreCase | RegexOptions.Multiline,
                TimeSpan.FromSeconds(2)
            );
            
            if (match.Success)
            {
                result.RawValue = match.Value.Trim();

                var textBeforeMatch = ocrResult.Text.Substring(0, match.Index);
                var pageCount = Regex.Matches(textBeforeMatch, PageSeparatorPattern).Count;
                result.PageNo = pageCount > 0 ? pageCount : 1;

                var metrics = _coordinateMapper.GetBoundingBoxForMatch(
                    match.Index, match.Length, ocrResult.Words);
                
                result.X = metrics.X;
                result.Y = metrics.Y;
                result.Width = metrics.Width;
                result.Height = metrics.Height;
                result.Confidence = metrics.AverageConfidence > 0 ? metrics.AverageConfidence : 0.90m;
            }
        }
        catch (RegexMatchTimeoutException ex)
        {
            _logger.LogWarning(ex, "Regex timeout for field {FieldName}", field.FieldName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Regex error for field {FieldName}", field.FieldName);
        }

        return result;
    }
}
