using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

/// <summary>
/// XML API:
/// POST /api/xml/create/{pdfId}  → XML üretir ve arşivler
/// GET  /api/xml/{pdfId}         → Kaydedilmiş XML'i getirir
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Numune Kabul Personeli,Admin")]
public class XmlController : ControllerBase
{
    private readonly IXmlService _xmlService;
    private readonly ILogger<XmlController> _logger;

    public XmlController(IXmlService xmlService, ILogger<XmlController> logger)
    {
        _xmlService = xmlService;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen PDF için XML üretir ve veritabanına arşivler.
    /// </summary>
    [HttpPost("create/{id}")]
    public async Task<IActionResult> CreateXml([FromRoute(Name = "id")] int pdfId)
    {
        try
        {
            var result = await _xmlService.CreateAndSaveAsync(pdfId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Id={pdfId} olan PDF bulunamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "XML üretimi sırasında hata oluştu. PdfId: {PdfId}", pdfId);
            return StatusCode(500, "XML üretimi sırasında sunucu hatası oluştu.");
        }
    }

    /// <summary>
    /// Belirtilen PDF için daha önce oluşturulmuş XML arşivini getirir.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetXml([FromRoute(Name = "id")] int pdfId)
    {
        var archive = await _xmlService.GetByPdfIdAsync(pdfId);
        if (archive == null)
            return NotFound($"Id={pdfId} için henüz oluşturulmuş bir XML arşivi bulunamadı.");

        return Ok(archive);
    }

    /// <summary>
    /// XML içeriğini direkt olarak XML formatında döner (indirme için).
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadXml([FromRoute(Name = "id")] int pdfId)
    {
        var archive = await _xmlService.GetByPdfIdAsync(pdfId);
        if (archive == null)
            return NotFound($"Id={pdfId} için henüz oluşturulmuş bir XML arşivi bulunamadı.");

        var bytes = System.Text.Encoding.UTF8.GetBytes(archive.XmlContent);
        return File(bytes, "application/xml", $"numune_kabul_{pdfId}.xml");
    }
}
