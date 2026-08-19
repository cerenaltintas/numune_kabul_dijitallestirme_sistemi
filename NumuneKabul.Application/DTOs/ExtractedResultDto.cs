namespace NumuneKabul.Application.DTOs;

// OCR sonucunda çıkarılan her bir alanın detaylarını taşır
public class ExtractedResultDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? RawValue { get; set; }
    public decimal Confidence { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int PageNo { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = "Success"; // Success, Warning, Danger
}
