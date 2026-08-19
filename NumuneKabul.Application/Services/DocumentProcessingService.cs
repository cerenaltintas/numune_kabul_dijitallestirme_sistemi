using Microsoft.Extensions.Logging;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Application.DTOs;
using System.Text.Json;

namespace NumuneKabul.Application.Services;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IPdfService _pdfService;
    private readonly IPdfImageService _pdfImageService;
    private readonly IOcrService _ocrService;
    private readonly IFormTemplateService _formTemplateService;
    private readonly IExtractionEngine _extractionEngine;
    private readonly IExtractedFieldService _extractedFieldService;
    private readonly IDocumentConfidenceScorer _confidenceScorer;
    private readonly ILogger<DocumentProcessingService> _logger;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly IAuditLogService _auditLogService;

    public DocumentProcessingService(
        IPdfService pdfService, 
        IPdfImageService pdfImageService, 
        IOcrService ocrService, 
        IFormTemplateService formTemplateService,
        IExtractionEngine extractionEngine,
        IExtractedFieldService extractedFieldService,
        IDocumentConfidenceScorer confidenceScorer,
        ILogger<DocumentProcessingService> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        IAuditLogService auditLogService)
    {
        _pdfService = pdfService;
        _pdfImageService = pdfImageService;
        _ocrService = ocrService;
        _formTemplateService = formTemplateService;
        _extractionEngine = extractionEngine;
        _extractedFieldService = extractedFieldService;
        _confidenceScorer = confidenceScorer;
        _logger = logger;
        _configuration = configuration;
        _auditLogService = auditLogService;
    }

    public async Task<string> ProcessDocumentAsync(int pdfId)
    {
        _logger.LogInformation("PDF İşleme süreci başlatıldı. PdfId: {PdfId}", pdfId);

        // 1. PDF'i bul
        var pdf = await _pdfService.GetPdfByIdAsync(pdfId);
        if (pdf == null)
        {
            throw new KeyNotFoundException($"Id={pdfId} olan PDF veritabanında bulunamadı.");
        }

        // 2. Resimlerin kaydedileceği klasör
        string outputFolder = $"pdf_{pdfId}";

        // 3. PDF'i Resimlere Çevir
        var imagePaths = await _pdfImageService.ConvertToImagesAsync(pdf.FilePath, outputFolder);

        // 4. Resimleri OCR'a Gönderip Oku (SLA ölçümü ile)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var ocrResult = await _ocrService.ExtractTextFromImagesAsync(imagePaths, pdf.TemplateId);
        sw.Stop();

        // Performans SLA Kontrolü: 10 saniyenin altında olmalı
        if (sw.ElapsedMilliseconds > 10000)
        {
            _logger.LogWarning("Performans SLA İhlali: OCR işlemi 10 saniyeyi aştı! Süre: {ElapsedMs} ms. PdfId: {PdfId}", sw.ElapsedMilliseconds, pdfId);
        }

        // 5. Yazıları ve koordinatları veritabanına kalıcı olarak kaydet
        await _pdfService.SaveOcrResultAsync(pdfId, ocrResult);

        // İşlem takibi için Audit Log (Veri sızdırmadan sadece metadata yazılır)
        await _auditLogService.LogAsync(
            action: "OCR_Completed",
            description: $"OCR işlemi başarıyla tamamlandı. Süre: {sw.ElapsedMilliseconds}ms, Ortalama Güven: {ocrResult.AverageConfidence:P2}",
            entityType: "PdfDocument",
            entityId: pdfId.ToString(),
            severity: sw.ElapsedMilliseconds > 10000 ? "Warning" : "Info",
            newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                AverageConfidence = ocrResult.AverageConfidence
            })
        );

        // 6. Extraction işlemi artık burada yapılmıyor, ayrı bir adımla tetiklenecek.

        _logger.LogInformation("PDF İşleme süreci (OCR) başarıyla tamamlandı. PdfId: {PdfId}", pdfId);

        return ocrResult.Text;
    }

    public async Task ExtractFieldsAsync(int pdfId, int institutionId, int templateId)
    {
        _logger.LogInformation("PDF için şablon uygulama süreci başlatıldı. PdfId: {PdfId}, TemplateId: {TemplateId}", pdfId, templateId);

        var pdf = await _pdfService.GetPdfByIdAsync(pdfId);
        if (pdf == null) throw new KeyNotFoundException($"Id={pdfId} olan PDF veritabanında bulunamadı.");
        int? oldTemplateId = pdf.TemplateId;

        // 1. PDF'i kuruma ve şablona ata
        await _pdfService.UpdateTemplateAsync(pdfId, institutionId, templateId);

        // 2. OCR Metnini ve Güven Skorunu getir
        var ocrResult = await _pdfService.GetSavedOcrDataAsync(pdfId);
        
        // EĞER ŞABLON DEĞİŞTİYSE VEYA İLK DEFA ATANIYORSA, ZONAL OCR İÇİN YENİDEN OCR YAP!
        if (oldTemplateId != templateId)
        {
            _logger.LogInformation("PDF şablonu değişti ({Old} -> {New}). OCR işlemi yeni şablona göre tekrarlanıyor...", oldTemplateId, templateId);
            
            string outputFolder = $"pdf_{pdfId}";
            string imageUploadFolder = _configuration["StorageSettings:ImagePath"] ?? "uploads/images";
            string targetFolder = Path.Combine(imageUploadFolder, outputFolder);
            string fullPath = Path.GetFullPath(targetFolder);
            
            if (Directory.Exists(fullPath))
            {
                // Sadece png uzantılı dosyaları al ve isme göre sırala (sayfa_1.png, sayfa_2.png vs.)
                var imagePaths = Directory.GetFiles(fullPath, "*.png").ToList();
                if (imagePaths.Any())
                {
                    ocrResult = await _ocrService.ExtractTextFromImagesAsync(imagePaths, templateId);
                    await _pdfService.SaveOcrResultAsync(pdfId, ocrResult);
                }
            }
        }

        if (ocrResult == null || string.IsNullOrWhiteSpace(ocrResult.Text))
        {
            throw new Exception("Bu belge için kaydedilmiş OCR metni bulunamadı.");
        }

        // 3. Şablonu getir ve kuralları uygula
        var template = await _formTemplateService.GetByIdAsync(templateId);
        if (template != null && template.TemplateFields.Any())
        {
            var extractedResults = _extractionEngine.ExtractFields(ocrResult, template.TemplateFields);
            
            foreach (var result in extractedResults)
            {
                var isRequired = template.TemplateFields.First(f => f.FieldName == result.FieldName).Required;
                if (result.RawValue != null)
                    _logger.LogInformation("OK - Alan: {Field}, Değer: {Value}, Skor: {Confidence}", result.FieldName, result.RawValue, result.Confidence);
                else if (isRequired)
                    _logger.LogWarning("EKSİK (Zorunlu) - Alan: {Field} OCR metninde bulunamadı!", result.FieldName);
            }

            // Veritabanına kaydet
            await _extractedFieldService.SaveResultsAsync(pdfId, extractedResults);

            // 4. Güven Skorunu Hesapla ve Kaydet
            decimal documentConfidenceScore = _confidenceScorer.CalculateDocumentScore(ocrResult.AverageConfidence, extractedResults, template.TemplateFields);
            
            await _pdfService.UpdateConfidenceScoreAsync(pdfId, documentConfidenceScore);
            
            _logger.LogInformation("Güven skoru hesaplandı: %{Score}. PdfId: {PdfId}", documentConfidenceScore, pdfId);
        }

        _logger.LogInformation("Şablon uygulama süreci tamamlandı. PdfId: {PdfId}", pdfId);
    }
}
