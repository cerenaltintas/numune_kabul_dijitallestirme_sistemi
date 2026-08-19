namespace NumuneKabul.Application.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string Severity { get; set; } = "Info";
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
