using NumuneKabul.Domain.Enums;

namespace NumuneKabul.Domain.Entities;

public class PdfDocument //belge
{
    public int Id { get; set; }
    public int? InstitutionId { get; set; }
    public int? TemplateId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = DocumentStatus.Uploaded.ToString();
    public int PageCount { get; set; }
    public decimal? ConfidenceScore { get; set; } // Belgenin genel güven skoru

    public Institution? Institution { get; set; }
    public FormTemplate? FormTemplate { get; set; }
}
