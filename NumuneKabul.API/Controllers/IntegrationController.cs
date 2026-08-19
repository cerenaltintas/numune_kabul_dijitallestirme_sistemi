using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

/// <summary>
/// �� Entegrasyon API:
/// POST /api/integration/send/{pdfId}    → Mock LIS/HBYS servisine gönderir
/// GET  /api/integration/status/{pdfId}  → Gönderim durumunu getirir
/// POST /api/integration/retry/{pdfId}   → Başarısız işi yeniden dener
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Numune Kabul Personeli,Admin,Entegrasyon Servisi")]
public class IntegrationController : ControllerBase
{
    private readonly IIntegrationService _integrationService;
    private readonly ILogger<IntegrationController> _logger;

    public IntegrationController(IIntegrationService integrationService, ILogger<IntegrationController> logger)
    {
        _integrationService = integrationService;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen PDF'i mock LIS/HBYS servisine gönderir.
    /// XML arşivi yoksa otomatik üretir.
    /// </summary>
    [HttpPost("send/{id}")]
    public async Task<IActionResult> SendToIntegration([FromRoute(Name = "id")] int pdfId)
    {
        try
        {
            var result = await _integrationService.SendToMockServiceAsync(pdfId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Id={pdfId} olan PDF bulunamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Entegrasyon gönderimi sırasında hata. PdfId: {PdfId}", pdfId);
            return StatusCode(500, "Entegrasyon gönderimi sırasında sunucu hatası oluştu.");
        }
    }

    /// <summary>
    /// Belirtilen PDF'in son entegrasyon gönderim durumunu getirir.
    /// </summary>
    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetStatus([FromRoute(Name = "id")] int pdfId)
    {
        var status = await _integrationService.GetJobStatusAsync(pdfId);
        if (status == null)
            return NotFound($"Id={pdfId} için henüz bir entegrasyon işi oluşturulmamış.");

        return Ok(status);
    }

    /// <summary>
    /// Başarısız entegrasyon işini yeniden dener (max 3 deneme).
    /// </summary>
    [HttpPost("retry/{id}")]
    public async Task<IActionResult> RetryIntegration([FromRoute(Name = "id")] int pdfId)
    {
        try
        {
            var result = await _integrationService.RetryJobAsync(pdfId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Id={pdfId} için yeniden denenecek bir entegrasyon işi bulunamadı.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Entegrasyon yeniden deneme sırasında hata. PdfId: {PdfId}", pdfId);
            return StatusCode(500, "Yeniden deneme sırasında sunucu hatası oluştu.");
        }
    }
}
