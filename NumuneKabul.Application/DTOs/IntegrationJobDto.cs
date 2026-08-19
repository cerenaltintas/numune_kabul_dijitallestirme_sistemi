namespace NumuneKabul.Application.DTOs;

//Entegre edilecek işlerin DTOsu

public class IntegrationJobDto
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    public string? ErrorMessage { get; set; }
}
