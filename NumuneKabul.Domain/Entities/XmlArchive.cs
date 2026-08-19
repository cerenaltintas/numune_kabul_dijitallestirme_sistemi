namespace NumuneKabul.Domain.Entities;

public class XmlArchive //xml arşivi
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string XmlContent { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public PdfDocument PdfDocument { get; set; } = null!;
}
