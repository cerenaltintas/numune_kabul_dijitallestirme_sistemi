namespace NumuneKabul.Domain.Entities;

public class IntegrationJob //işleri sıraya alma
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string Status { get; set; } = "Pending";
    public int RetryCount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastAttemptDate { get; set; }
    public string? ErrorMessage { get; set; }

    public PdfDocument PdfDocument { get; set; } = null!;
}
