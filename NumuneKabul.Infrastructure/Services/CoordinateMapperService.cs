using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services;

public class CoordinateMapperService : ICoordinateMapperService
{
    public MatchMetricsDto GetBoundingBoxForMatch(
        int matchIndex, 
        int matchLength, 
        List<OcrWordDto> ocrWords)
    {
        if (ocrWords == null || !ocrWords.Any())
            return new MatchMetricsDto();

        // 1. matchIndex ile örtüşen kelimeleri bul
        var matchedWords = ocrWords
            .Where(w => w.StartIndex + w.Text.Length > matchIndex && 
                        w.StartIndex < matchIndex + matchLength)
            .ToList();

        if (!matchedWords.Any()) return new MatchMetricsDto();

        // 2. Bulunan kelimelerin kapsayıcı çerçevesini (Bounding Box) birleştir
        int minX = matchedWords.Min(w => w.X);
        int minY = matchedWords.Min(w => w.Y);
        int maxX = matchedWords.Max(w => w.X + w.Width);
        int maxY = matchedWords.Max(w => w.Y + w.Height);
        decimal avgConfidence = matchedWords.Average(w => w.Confidence);

        return new MatchMetricsDto
        {
            X = minX,
            Y = minY,
            Width = maxX - minX,
            Height = maxY - minY,
            AverageConfidence = avgConfidence
        };
    }
}
