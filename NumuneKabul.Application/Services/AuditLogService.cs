using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;

using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace NumuneKabul.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<AuditLogDto>> GetLogsByEntityAsync(string entityType, string entityId)
    {
        var logs = await _unitOfWork.AuditLogs.FindAsync(a => a.EntityType == entityType && a.EntityId == entityId);

        return logs.OrderByDescending(a => a.Date).Select(a => new AuditLogDto
        {
            Id = a.Id,
            UserId = a.UserId,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Action = a.Action,
            Date = a.Date,
            Description = a.Description,
            IpAddress = a.IpAddress,
            Severity = a.Severity,
            OldValues = a.OldValues,
            NewValues = a.NewValues
        }).ToList();
    }

    /// <summary>
    /// Merkezi log yazma noktası. Ip ve UserId Context üzerinden alınır.
    /// </summary>
    public async Task LogAsync(string action, string description, string entityType, string entityId, string severity = "Info", string? oldValues = null, string? newValues = null)
    {
        var context = _httpContextAccessor.HttpContext;
        var ipAddress = context?.Connection?.RemoteIpAddress?.ToString();
        
        int? userId = null;
        var nameIdentifier = context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(nameIdentifier, out int parsedUserId))
        {
            userId = parsedUserId;
        }

        var auditLog = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Description = description,
            Date = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserId = userId,
            Severity = severity,
            OldValues = oldValues,
            NewValues = newValues
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        // SaveChanges sorumluluğu UnitOfWork'u kullanan ana servise devredildi.
    }
}

