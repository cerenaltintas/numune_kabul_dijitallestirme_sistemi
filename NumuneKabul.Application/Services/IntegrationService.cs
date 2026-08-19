using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Enums;
using NumuneKabul.Domain.Interfaces;

namespace NumuneKabul.Application.Services;

/// <summary>
/// Entegrasyon: Mock REST servise XML gönderir.
/// İlk sürümde REST API üzerinden XML gönderilecektir.
/// RetryCount max 3, başarısız job yeniden denenebilir.
/// </summary>
public class IntegrationService : IIntegrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IXmlService _xmlService;
    private readonly IXmlMappingService _xmlMappingService;
    private readonly IIntegrationAdapter _integrationAdapter;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<IntegrationService> _logger;
    private const int MaxRetryCount = 3;

    public IntegrationService(
        IUnitOfWork unitOfWork,
        IXmlService xmlService,
        IXmlMappingService xmlMappingService,
        IIntegrationAdapter integrationAdapter,
        IAuditLogService auditLogService,
        ILogger<IntegrationService> logger)
    {
        _unitOfWork = unitOfWork;
        _xmlService = xmlService;
        _xmlMappingService = xmlMappingService;
        _integrationAdapter = integrationAdapter;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<IntegrationJobDto> SendToMockServiceAsync(int pdfDocumentId)
    {
        _logger.LogInformation("Entegrasyon gönderimi başlatıldı. PdfId: {PdfId}", pdfDocumentId);

        // 1. PDF var mı kontrol et
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(pdfDocumentId)
            ?? throw new KeyNotFoundException($"Id={pdfDocumentId} olan PDF bulunamadı.");

        // 2. XML arşivi var mı? Yoksa üret
        var xmlArchive = await _xmlService.GetByPdfIdAsync(pdfDocumentId);
        if (xmlArchive == null)
        {
            _logger.LogInformation("XML arşivi bulunamadı, otomatik üretiliyor. PdfId: {PdfId}", pdfDocumentId);
            await _xmlService.CreateAndSaveAsync(pdfDocumentId);
            xmlArchive = await _xmlService.GetByPdfIdAsync(pdfDocumentId);
        }

        // 3. IntegrationJob oluştur
        var job = new IntegrationJob
        {
            PdfDocumentId = pdfDocumentId,
            Status = IntegrationStatus.Sending.ToString(),
            RetryCount = 0,
            CreatedDate = DateTime.UtcNow,
            LastAttemptDate = DateTime.UtcNow
        };
        await _unitOfWork.IntegrationJobs.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();

        // 4. Mock servise gönderim öncesi format dönüşümü (Mapping)
        var mappedXml = _xmlMappingService.MapToTargetFormat(xmlArchive!.XmlContent, "MockRest");

        // 5. Adaptör üzerinden dış sisteme (veya mock servise) gönderim yap
        bool success = await _integrationAdapter.SendAsync(mappedXml, pdfDocumentId);

        // 6. Sonucu kaydet
        job.Status = success ? IntegrationStatus.Sent.ToString() : IntegrationStatus.Failed.ToString();
        job.LastAttemptDate = DateTime.UtcNow;
        if (!success)
            job.ErrorMessage = "Mock servis geçici olarak kullanılamıyor.";

        _unitOfWork.IntegrationJobs.Update(job);

        // 7. PDF durumunu güncelle
        pdf.Status = success
            ? DocumentStatus.IntegrationSent.ToString()
            : DocumentStatus.IntegrationFailed.ToString();
        _unitOfWork.PdfDocuments.Update(pdf);

        await _unitOfWork.SaveChangesAsync();

        // 8. Audit log yaz
        await _auditLogService.LogAsync(
            action: job.Status,
            description: $"[PdfId: {pdfDocumentId}] Mock servise gönderim: {(success ? "Başarılı" : "Başarısız")}",
            entityType: "PdfDocument",
            entityId: pdfDocumentId.ToString(),
            severity: success ? "Info" : "Error");

        _logger.LogInformation("Entegrasyon gönderimi tamamlandı. PdfId: {PdfId}, Durum: {Status}", pdfDocumentId, job.Status);

        return MapToDto(job);
    }

    public async Task<IntegrationJobDto?> GetJobStatusAsync(int pdfDocumentId)
    {
        var jobs = await _unitOfWork.IntegrationJobs.FindAsync(j => j.PdfDocumentId == pdfDocumentId);
        var latest = jobs.OrderByDescending(j => j.CreatedDate).FirstOrDefault();
        return latest == null ? null : MapToDto(latest);
    }

    public async Task<IntegrationJobDto> RetryJobAsync(int pdfDocumentId)
    {
        var jobs = await _unitOfWork.IntegrationJobs.FindAsync(j => j.PdfDocumentId == pdfDocumentId);
        var job = jobs.OrderByDescending(j => j.CreatedDate).FirstOrDefault()
            ?? throw new KeyNotFoundException($"Id={pdfDocumentId} için entegrasyon işi bulunamadı.");

        if (job.RetryCount >= MaxRetryCount)
            throw new InvalidOperationException($"Maksimum yeniden deneme sayısına ({MaxRetryCount}) ulaşıldı.");

        job.RetryCount++;
        job.Status = IntegrationStatus.Retrying.ToString();
        job.LastAttemptDate = DateTime.UtcNow;
        job.ErrorMessage = null;
        _unitOfWork.IntegrationJobs.Update(job);
        await _unitOfWork.SaveChangesAsync();

        // Tekrar gönder
        return await SendToMockServiceAsync(pdfDocumentId);
    }

    private static IntegrationJobDto MapToDto(IntegrationJob job) => new()
    {
        Id = job.Id,
        PdfDocumentId = job.PdfDocumentId,
        Status = job.Status,
        RetryCount = job.RetryCount,
        CreatedDate = job.CreatedDate,
        LastAttemptDate = job.LastAttemptDate,
        ErrorMessage = job.ErrorMessage
    };
}
