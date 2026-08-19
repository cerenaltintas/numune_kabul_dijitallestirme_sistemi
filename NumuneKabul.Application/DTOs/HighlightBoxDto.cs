namespace NumuneKabul.Application.DTOs;

public class HighlightBoxDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string ColorHex { get; set; } = "#FF0000"; // Varsayılan Kırmızı
    public string Label { get; set; } = string.Empty; // Kutunun üzerine yazılacak metin (Opsiyonel)
}
