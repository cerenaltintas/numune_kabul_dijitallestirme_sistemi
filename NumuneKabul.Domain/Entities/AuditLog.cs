namespace NumuneKabul.Domain.Entities;

public class AuditLog //logları tutar
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
    
    // Güvenlik ve Adli Analiz
    public string? IpAddress { get; set; }
    public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
    
    // Yapısal Veri (JSON)
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public User? User { get; set; }
}
