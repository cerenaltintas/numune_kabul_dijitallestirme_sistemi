namespace NumuneKabul.Application.DTOs;

// Oluşturulan XML paketlerinin arayüzdeki arşiv listesinde (indirme linki, tarih vb.) gösterilmesi için 
public class XmlArchiveDto
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string XmlContent { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class XmlCreateResultDto
{
    public int ArchiveId { get; set; }
    public int PdfDocumentId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
