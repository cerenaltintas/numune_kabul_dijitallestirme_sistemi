using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

/// <summary>
/// OCR API:
/// POST /api/ocr/start/{pdfId}   → OCR işlemini başlatır
/// GET  /api/ocr/result/{pdfId}  → Kaydedilmiş OCR sonucunu getirir
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Numune Kabul Personeli,Admin")]
public class OcrController : ControllerBase
{
    private readonly IDocumentProcessingService _documentProcessingService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<OcrController> _logger;

    public OcrController(
        IDocumentProcessingService documentProcessingService,
        IPdfService pdfService,
        ILogger<OcrController> logger)
    {
        _documentProcessingService = documentProcessingService;
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// PDF'i OCR ile işler: resme dönüştürür, metin çıkarır, şablon varsa alanları ayıklar ve DB'ye kaydeder.
    /// </summary>
    [HttpPost("start/{id}")]
    public async Task<IActionResult> StartOcr([FromRoute(Name = "id")] int pdfId)
    {
        try
        {
            _logger.LogInformation("OCR işlemi başlatılıyor. PdfId: {PdfId}", pdfId);
            await _documentProcessingService.ProcessDocumentAsync(pdfId);
            return Ok(new { message = "OCR işlemi başarıyla tamamlandı.", pdfId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Id={pdfId} olan PDF bulunamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR işlemi sırasında hata oluştu. PdfId: {PdfId}", pdfId);
            // G3: İç hata mesajı client'a sızdırılmıyor
            return StatusCode(500, "OCR işlemi sırasında sunucu hatası oluştu.");
        }
    }

    /// <summary>
    /// Belirtilen PDF için veritabanına kaydedilmiş OCR metnini getirir.
    /// </summary>
    [HttpGet("result/{id}")]
    public async Task<IActionResult> GetOcrResult([FromRoute(Name = "id")] int pdfId)
    {
        var data = await _pdfService.GetSavedOcrDataAsync(pdfId);
        var text = data?.Text;
        if (string.IsNullOrEmpty(text))
            return NotFound("Bu PDF için henüz kaydedilmiş OCR sonucu bulunamadı.");

        return Ok(new { pdfId, ocrText = text });
    }
}
