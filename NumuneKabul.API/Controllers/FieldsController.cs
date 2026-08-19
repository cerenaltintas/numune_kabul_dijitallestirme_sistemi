using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

/// <summary>
/// MD API tasarımı:
/// GET /api/fields/{id}  → Çıkarılmış alanları getir
/// PUT /api/fields/{id}     → Manuel düzelt (Kullanıcı eksik alanları düzeltir)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Numune Kabul Personeli,Admin")]
public class FieldsController : ControllerBase
{
    private readonly IExtractedFieldService _extractedFieldService;
    private readonly ILogger<FieldsController> _logger;

    public FieldsController(IExtractedFieldService extractedFieldService, ILogger<FieldsController> logger)
    {
        _extractedFieldService = extractedFieldService;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen PDF dokümanı için çıkarılmış tüm alanları getirir.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<List<ExtractedResultDto>>> GetResultsByPdfId(int id)
    {
        var results = await _extractedFieldService.GetByPdfIdAsync(id);

        if (results == null || !results.Any())
            return NotFound("Bu dokümana ait çıkarılmış bir veri bulunamadı.");

        return Ok(results);
    }

    /// <summary>
    /// Tek bir çıkarılmış alanın değerini kullanıcı tarafından düzeltilmiş değerle günceller.
    /// "Her değişiklik Audit Log'a yazılacaktır."
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateField(int id, [FromBody] UpdateExtractedFieldDto dto)
    {
        if (id != dto.Id)
            return BadRequest("URL'deki ID ile gövdedeki ID eşleşmiyor.");

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "1";
        int userId = int.Parse(userIdStr);

        var result = await _extractedFieldService.UpdateFieldAsync(id, dto, userId);
        if (!result)
            return NotFound($"Id={id} olan çıkarılmış alan bulunamadı.");

        return Ok(new { message = "Alan başarıyla güncellendi.", fieldId = id });
    }

    /// <summary>
    /// Bir belgeye ait birden fazla alanı tek seferde düzeltir (Toplu Kayıt).
    /// </summary>
    [HttpPost("{id}/corrections")]
    public async Task<IActionResult> BulkUpdateFields(int id, [FromBody] List<UpdateExtractedFieldDto> corrections)
    {
        if (corrections == null || !corrections.Any())
            return BadRequest("Düzeltilecek alanlar listesi boş olamaz.");

        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "1";
            int userId = int.Parse(userIdStr);

            await _extractedFieldService.SaveCorrectionsAsync(id, corrections, userId);
            return Ok(new { message = $"{corrections.Count} alan başarıyla güncellendi.", id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu düzeltme kaydedilirken hata. PdfId: {PdfId}", id);
            return StatusCode(500, "Düzeltmeler kaydedilirken sunucu hatası oluştu.");
        }
    }
}
