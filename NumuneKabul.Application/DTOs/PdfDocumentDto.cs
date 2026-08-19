namespace NumuneKabul.Application.DTOs;

// Arayüzdeki pdf listesini çizerken kullanılan, kurum adlarının kolaylık için düzleştirilir
public class PdfDocumentDto
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public decimal? ConfidenceScore { get; set; }
}
