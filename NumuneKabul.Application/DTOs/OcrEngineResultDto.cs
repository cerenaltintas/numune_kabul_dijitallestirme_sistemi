namespace NumuneKabul.Application.DTOs;

public class OcrEngineResultDto
{
    public string Text { get; set; } = string.Empty;
    public decimal AverageConfidence { get; set; }
    public List<OcrWordDto> Words { get; set; } = new();
}
