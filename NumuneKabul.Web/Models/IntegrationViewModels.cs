namespace NumuneKabul.Web.Models;

/// <summary>API'den dönen login yanıtı — Web katmanı ViewModel'ı.</summary>
public class LoginResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? InstitutionId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>XML arşivi ViewModel'ı.</summary>
public class XmlArchiveViewModel
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string XmlContent { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

/// <summary>Entegrasyon iş durumu ViewModel'ı.</summary>
public class IntegrationJobViewModel
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastAttemptDate { get; set; }

    public bool CanRetry => Status == "Failed" && RetryCount < 3;
}
