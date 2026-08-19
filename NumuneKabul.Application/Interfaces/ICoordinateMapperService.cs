using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

public interface ICoordinateMapperService
{
    /// <summary>
    /// Regex ile bulunan metnin başlangıç indeksi ve uzunluğuna göre,
    /// o metni kapsayan Bounding Box koordinatlarını hesaplar.
    /// </summary>
    MatchMetricsDto GetBoundingBoxForMatch(
        int matchIndex, 
        int matchLength, 
        List<OcrWordDto> ocrWords);
}
