namespace NumuneKabul.Web.Models;

public class PdfDocumentViewModel
{
    public int Id { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string? TemplateName { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public decimal? ConfidenceScore { get; set; }
}
