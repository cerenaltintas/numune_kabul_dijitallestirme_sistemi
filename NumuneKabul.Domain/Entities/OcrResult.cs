namespace NumuneKabul.Domain.Entities;

public class OcrResult //ham veriler
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string? RawWordsJson { get; set; } // OCR'dan elde edilen kelimeler ve koordinatlarının JSON formatı
    public decimal AverageConfidence { get; set; } // Tesseract'ın sayfalardan hesapladığı ortalama güven skoru
    public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;

    public PdfDocument PdfDocument { get; set; } = null!;
}
