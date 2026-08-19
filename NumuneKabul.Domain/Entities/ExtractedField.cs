namespace NumuneKabul.Domain.Entities;

public class ExtractedField //alanlar, OCR sonucu tutulur.
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? RawValue { get; set; }
    public string? CorrectedValue { get; set; } //düzeltilmiş değer
    public decimal Confidence { get; set; } //güven yüzdesi
    public int PageNo { get; set; }
    // pdf içindeki koordinatları göstermek için
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Status { get; set; } = "Pending";

    public PdfDocument PdfDocument { get; set; } = null!;
}
