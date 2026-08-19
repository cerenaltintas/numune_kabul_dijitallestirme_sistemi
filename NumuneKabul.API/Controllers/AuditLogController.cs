using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Bir belge üzerindeki tüm işlemleri ve manuel düzenlemeleri (Audit Log) getirir.
    /// </summary>
    [HttpGet("document/{pdfId}")]
    public async Task<ActionResult<List<AuditLogDto>>> GetLogsByPdfId(int pdfId)
    {
        var logs = await _auditLogService.GetLogsByEntityAsync("PdfDocument", pdfId.ToString());

        if (logs == null || !logs.Any())
            return NotFound("Bu belgeye ait bir işlem geçmişi bulunamadı.");

        return Ok(logs);
    }
}
