using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentPreviewController : ControllerBase
{
    private readonly IExtractedFieldService _extractedFieldService;
    private readonly IPdfImageService _pdfImageService;

    public DocumentPreviewController(
        IExtractedFieldService extractedFieldService,
        IPdfImageService pdfImageService)
    {
        _extractedFieldService = extractedFieldService;
        _pdfImageService = pdfImageService;
    }

    /// <summary>
    /// PDF'in belirli bir sayfasını işaretsiz ham resim (PNG/JPEG) olarak döner.
    /// UI tarafında Frontend üzerinden (JS/CSS ile) etkileşimli kutu çizimi yapılacağı zaman kullanılır.
    /// </summary>
    [HttpGet("{pdfId}/page/{pageNo}/clean")]
    public IActionResult GetCleanPage(int pdfId, int pageNo)
    {
        string imagePath = _pdfImageService.GetImageFilePath(pdfId, pageNo);
        if (!System.IO.File.Exists(imagePath))
            return NotFound("İlgili sayfanın resmi bulunamadı.");

        // Resmi stream olarak dön
        var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, "image/jpeg");
    }
}
