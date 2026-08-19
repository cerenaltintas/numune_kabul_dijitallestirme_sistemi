using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Web.Models;
using NumuneKabul.Web.Services;

using NumuneKabul.Web.Filters;

namespace NumuneKabul.Web.Controllers;

[SessionAuthorize(Roles = "Numune Kabul Personeli,Admin")]
public class DocumentController : Controller
{
    private readonly IApiClientService _apiClient;

    public DocumentController(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    // GET: /Document
    public async Task<IActionResult> Index(int page = 1)
    {
        // Sayfa başına 10 kayıt gösteriyoruz
        var result = await _apiClient.GetPdfsPaginatedAsync(page, 10);
        
        if (result == null)
        {
            result = new PaginatedResultViewModel<PdfDocumentViewModel>();
            TempData["ErrorMessage"] = "PDF listesi yüklenirken bir hata oluştu.";
        }

        return View(result);
    }

    // GET: /Document/Upload
    [HttpGet]
    public IActionResult Upload()
    {
        return View(new PdfUploadViewModel());
    }

    // POST: /Document/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(52428800)] // 50 MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
    public async Task<IActionResult> Upload(PdfUploadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var uploadedPdfId = await _apiClient.UploadPdfAsync(model);

        if (uploadedPdfId.HasValue)
        {
            TempData["SuccessMessage"] = "PDF başarıyla yüklendi. OCR işlemi arka planda başlatıldı.";
            return RedirectToAction(nameof(Viewer), new { id = uploadedPdfId.Value });
        }

        ModelState.AddModelError("", "PDF yüklenirken bir hata oluştu. Lütfen tekrar deneyin.");
        return View(model);
    }

    // GET: /Document/GetTemplates
    [HttpGet]
    public async Task<IActionResult> GetTemplates(int institutionId)
    {
        var templates = await _apiClient.GetTemplatesByInstitutionAsync(institutionId);
        return Json(templates);
    }

    // GET: /Document/View/{id}
    public async Task<IActionResult> Viewer(int id, int page = 1)
    {
        var pdf = await _apiClient.GetPdfByIdAsync(id);
        if (pdf == null) return NotFound("Dosya bulunamadı.");
        
        ViewBag.CurrentPage = page;

        if (pdf.Status == "OcrCompleted" || pdf.Status == "NeedsManualReview" || pdf.Status == "Corrected")
        {
            var ocrResult = await _apiClient.GetOcrResultAsync(id);
            ViewBag.OcrText = ocrResult?.KaydedilenYazi;

            // Alanları (Extracted Fields) çek
            var extractedFields = await _apiClient.GetExtractedFieldsAsync(id);
            ViewBag.ExtractedFields = extractedFields.ToList();

            // Şablon uygulamak için kurumları çek
            ViewBag.Institutions = await _apiClient.GetInstitutionsAsync();
        }

        return View(pdf);
    }

    // GET: /Document/PreviewImage/{id} (Görüntüleyici için Renkli İşaretlenmiş Resim)
    [HttpGet]
    public async Task<IActionResult> PreviewImage(int id, int page = 1)
    {
        var stream = await _apiClient.GetHighlightedImageStreamAsync(id, page);
        if (stream == null) 
        {
            // Eğer renkli resim henüz yoksa, normal PDF'i dön
            return await StreamPdf(id);
        }

        return File(stream, "image/jpeg");
    }

    // GET: /Document/CleanPageImage/{id} (Etkileşimli Kutu Çizimi için Temiz Resim)
    [HttpGet]
    public async Task<IActionResult> CleanPageImage(int id, int page = 1)
    {
        var stream = await _apiClient.GetCleanImageStreamAsync(id, page);
        if (stream == null) 
        {
            return NotFound("Resim bulunamadı.");
        }

        return File(stream, "image/jpeg");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateField(int pdfId, int fieldId, string? correctedValue)
    {
        var isSuccess = await _apiClient.UpdateExtractedFieldAsync(pdfId, fieldId, correctedValue, "Web üzerinden düzeltildi");
        
        bool isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        
        if (isAjax)
        {
            if (isSuccess) return Json(new { success = true, message = "Alan başarıyla güncellendi." });
            return Json(new { success = false, message = "Alan güncellenirken bir hata oluştu." });
        }

        if (isSuccess)
        {
            TempData["SuccessMessage"] = "Alan başarıyla güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Alan güncellenirken bir hata oluştu.";
        }
        
        return RedirectToAction(nameof(Viewer), new { id = pdfId });
    }

    // GET: /Document/AuditLogs/{id}
    [HttpGet]
    public async Task<IActionResult> AuditLogs(int id)
    {
        var logs = await _apiClient.GetAuditLogsAsync(id);
        return PartialView("_AuditLogsPartial", logs);
    }

    // GET: /Document/StreamPdf/{id} (Görüntüleyici için API Proxy)
    [HttpGet]
    public async Task<IActionResult> StreamPdf(int id)
    {
        var stream = await _apiClient.GetPdfStreamAsync(id);
        if (stream == null) return NotFound("Dosya bulunamadı.");

        return File(stream, "application/pdf");
    }

    // POST: /Document/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var isSuccess = await _apiClient.DeletePdfAsync(id);
        if (isSuccess)
        {
            TempData["SuccessMessage"] = "PDF başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Silme işlemi başarısız oldu.";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Document/OcrResult/{id}
    public async Task<IActionResult> OcrResult(int id)
    {
        var result = await _apiClient.GetOcrResultAsync(id);
        if (result == null)
        {
            TempData["ErrorMessage"] = "Bu PDF için henüz bir OCR sonucu kaydedilmemiş veya bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Institutions = await _apiClient.GetInstitutionsAsync();
        return View(result);
    }

    // POST: /Document/ApplyTemplate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyTemplate(int pdfId, int institutionId, int templateId)
    {
        var isSuccess = await _apiClient.ApplyTemplateAsync(pdfId, institutionId, templateId);
        if (isSuccess)
        {
            TempData["SuccessMessage"] = "Şablon başarıyla uygulandı ve veriler çıkarıldı.";
        }
        else
        {
            TempData["ErrorMessage"] = "Şablon uygulanırken bir hata oluştu.";
        }
        
        return RedirectToAction(nameof(Viewer), new { id = pdfId });
    }

    // POST: /Document/RetryOcr/{id} (Yeniden OCR veya İlk Kez OCR Tetikleme)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryOcr(int id)
    {
        var isSuccess = await _apiClient.StartOcrAsync(id);
        if (isSuccess)
        {
            TempData["SuccessMessage"] = "OCR işlemi başarıyla çalıştırıldı ve sonuçlar kaydedildi.";
        }
        else
        {
            TempData["ErrorMessage"] = "OCR işlemi sırasında bir hata oluştu.";
        }
        
        return RedirectToAction(nameof(Viewer), new { id = id });
    }

    // ─── XML ve Entegrasyon Action'ları

    // POST: /Document/CreateXml/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateXml(int id)
    {
        var isSuccess = await _apiClient.CreateXmlAsync(id);
        TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = isSuccess
            ? "XML başarıyla oluşturuldu ve arşivlendi."
            : "XML oluşturulurken bir hata oluştu.";
        return RedirectToAction(nameof(Viewer), new { id });
    }

    // GET: /Document/DownloadXml/{id}
    [HttpGet]
    public async Task<IActionResult> DownloadXml(int id)
    {
        var stream = await _apiClient.GetXmlDownloadStreamAsync(id);
        if (stream == null)
        {
            TempData["ErrorMessage"] = "Bu belge için henüz XML oluşturulmamış.";
            return RedirectToAction(nameof(Viewer), new { id });
        }
        return File(stream, "application/xml", $"numune_kabul_{id}.xml");
    }

    // POST: /Document/SendToIntegration/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToIntegration(int id)
    {
        var isSuccess = await _apiClient.SendToIntegrationAsync(id);
        TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = isSuccess
            ? "Belge LIS/HBYS sistemine başarıyla gönderildi."
            : "Gönderim sırasında bir hata oluştu. Lütfen tekrar deneyin.";
        return RedirectToAction(nameof(Viewer), new { id });
    }

    // POST: /Document/RetryIntegration/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryIntegration(int id)
    {
        var isSuccess = await _apiClient.RetryIntegrationAsync(id);
        TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = isSuccess
            ? "Yeniden gönderim başarılı."
            : "Yeniden gönderim başarısız veya maksimum deneme sayısına ulaşıldı.";
        return RedirectToAction(nameof(Viewer), new { id });
    }
}
