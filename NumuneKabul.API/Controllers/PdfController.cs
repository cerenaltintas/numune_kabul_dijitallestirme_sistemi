using NumuneKabul.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NumuneKabul.Application.DTOs;
using System.Security.Claims;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Numune Kabul Personeli,Admin")]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<PdfController> _logger;
    private readonly IDocumentProcessingService _documentProcessingService;

    public PdfController(IPdfService pdfService, IDocumentProcessingService documentProcessingService, ILogger<PdfController> logger)
    {
        _pdfService = pdfService;
        _documentProcessingService = documentProcessingService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52428800)] // 50 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] int? institutionId, [FromForm] int? templateId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Lütfen geçerli bir PDF dosyası seçin.");

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Sadece PDF dosyaları yüklenebilir.");

        // G4: Magic Bytes doğrulaması — uzantı sahteliğini engeller
        if (!await IsPdfFileAsync(file))
            return BadRequest("Yüklenen dosya geçerli bir PDF formatında değil.");

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _pdfService.UploadPdfAsync(stream, file.FileName, institutionId, templateId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Geçersiz parametre: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF yüklenirken hata oluştu.");
            return StatusCode(500, "Sunucu hatası: PDF yüklenemedi.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Yetki kontrolü: User rolü sadece kendi kurumunun PDF'lerini görebilir.
        var role = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
        var institutionClaim = User.FindFirstValue("InstitutionId");

        if (role != "Admin" && int.TryParse(institutionClaim, out var institutionId))
        {
            var filteredPdfs = await _pdfService.GetPdfsByInstitutionAsync(institutionId);
            return Ok(filteredPdfs);
        }

        var pdfs = await _pdfService.GetAllPdfsAsync();
        return Ok(pdfs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pdf = await _pdfService.GetPdfByIdAsync(id);
        if (pdf == null)
            return NotFound("PDF bulunamadı.");

        return Ok(pdf);
    }

    [HttpGet("institution/{institutionId}")]
    public async Task<IActionResult> GetByInstitution(int institutionId)
    {
        var pdfs = await _pdfService.GetPdfsByInstitutionAsync(institutionId);
        return Ok(pdfs);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var pdfs = await _pdfService.GetPdfsByStatusAsync(status);
        return Ok(pdfs);
    }

    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        // Yetki kontrolü: User rolü sadece kendi kurumunun belgelerini görebilir.
        var role = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
        var institutionClaim = User.FindFirstValue("InstitutionId");

        if (role != "Admin" && int.TryParse(institutionClaim, out var institutionId))
        {
            var filteredResult = await _pdfService.GetPdfsPaginatedByInstitutionAsync(page, pageSize, institutionId);
            return Ok(filteredResult);
        }

        var result = await _pdfService.GetPdfsPaginatedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await _pdfService.DownloadPdfAsync(id);
        if (result == null)
            return NotFound("Dosya bulunamadı.");

        return File(result.Value.FileStream, "application/pdf", result.Value.FileName);
    }

    [HttpGet("{id}/view")]
    public async Task<IActionResult> ViewFile(int id)
    {
        var result = await _pdfService.DownloadPdfAsync(id);
        if (result == null)
            return NotFound("Dosya bulunamadı.");

        return File(result.Value.FileStream, "application/pdf");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _pdfService.DeletePdfAsync(id);
        if (!result)
            return NotFound("PDF bulunamadı.");

        return NoContent();
    }

    [HttpGet("{id}/ocr-result")]
    public async Task<IActionResult> GetOcrResult(int id)
    {
        var data = await _pdfService.GetSavedOcrDataAsync(id);
        var savedText = data?.Text;
        if (string.IsNullOrEmpty(savedText))
            return NotFound("Bu PDF için henüz kaydedilmiş bir OCR sonucu bulunamadı.");

        return Ok(new
        {
            PdfId = id,
            KaydedilenYazi = savedText
        });
    }

    [HttpPost("{id}/extract")]
    public async Task<IActionResult> Extract(int id, [FromQuery] int institutionId, [FromQuery] int templateId)
    {
        try
        {
            // GEÇİCİ ÇÖZÜM: Daha önce Deskew/Padding olmadan kaydedilmiş eski OCR 
            // verilerinin veritabanında kalmasını (cache) önlemek için, 
            // Şablon Uygula butonuna basıldığında OCR'ı YENİDEN zorla çalıştırıyoruz.
            await _documentProcessingService.ProcessDocumentAsync(id);
            
            await _documentProcessingService.ExtractFieldsAsync(id, institutionId, templateId);
            return Ok(new { Message = "Veri çıkarma işlemi başarıyla tamamlandı." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veri çıkarma işlemi sırasında hata oluştu. PdfId: {PdfId}", id);
            return StatusCode(500, "Sunucu hatası: Veri çıkarılamadı.");
        }
    }

    /// <summary>
    /// Dosyanın gerçekten PDF olup olmadığını magic bytes kontrolüyle doğrular.
    /// Yalnızca uzantı değiştirilerek yüklenen zararlı dosyaları engeller.
    /// </summary>
    private static async Task<bool> IsPdfFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var buffer = new byte[4];
        var bytesRead = await stream.ReadAsync(buffer, 0, 4);
        // PDF magic bytes: %PDF = 0x25 0x50 0x44 0x46
        return bytesRead == 4
               && buffer[0] == 0x25  // %
               && buffer[1] == 0x50  // P
               && buffer[2] == 0x44  // D
               && buffer[3] == 0x46; // F
    }
}
