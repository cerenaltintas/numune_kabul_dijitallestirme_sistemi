namespace NumuneKabul.Application.DTOs;

public class OcrWordDto
{
    public string Text { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int PageNo { get; set; }
    public decimal Confidence { get; set; }
}
