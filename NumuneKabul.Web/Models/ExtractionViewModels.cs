namespace NumuneKabul.Web.Models;

public class ExtractedFieldViewModel
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? RawValue { get; set; }
    public decimal Confidence { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int PageNo { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Severity { get; set; } = "Success"; // Success, Warning, Danger
}

public class AuditLogViewModel
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
