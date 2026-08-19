using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Audit log okuma ve yazma işlemleri için tek merkezi nokta.
/// Servisler log yazmak için bu arayüzü kullanır,
/// böylece altyapıdaki log mekanizmasına (Serilog, DB vs.) doğrudan bağlanmazlar.
/// </summary>
public interface IAuditLogService
{
    /// <summary>Belirli bir varlığa ait audit logları getirir.</summary>
    Task<List<AuditLogDto>> GetLogsByEntityAsync(string entityType, string entityId);

    /// <summary>
    /// Merkezi log yazma metodu. Ip adresi ve UserId HTTP context'ten otomatik çekilir.
    /// </summary>
    Task LogAsync(string action, string description, string entityType, string entityId, string severity = "Info", string? oldValues = null, string? newValues = null);
}

