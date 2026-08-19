using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Enums;
using NumuneKabul.Domain.Interfaces;

namespace NumuneKabul.Application.Services;

/// <summary>
/// XML: Her belge için XML üretir.
/// XML içeriği: OCR metni, düzeltilmiş alanlar, güven skorları, koordinatlar, belge bilgileri.
/// </summary>
public class XmlService : IXmlService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly IXmlBuilder _xmlBuilder;
    private readonly ILogger<XmlService> _logger;

    public XmlService(IUnitOfWork unitOfWork, IAuditLogService auditLogService, IXmlBuilder xmlBuilder, ILogger<XmlService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _xmlBuilder = xmlBuilder;
        _logger = logger;
    }

    public async Task<XmlCreateResultDto> CreateAndSaveAsync(int pdfDocumentId)
    {
        _logger.LogInformation("XML üretimi başlatıldı. PdfId: {PdfId}", pdfDocumentId);

        // 1. PDF belgesi bilgilerini getir
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(pdfDocumentId)
            ?? throw new KeyNotFoundException($"Id={pdfDocumentId} olan PDF bulunamadı.");

        // 2. OCR metnini getir
        var ocrResults = await _unitOfWork.OcrResults.FindAsync(r => r.PdfDocumentId == pdfDocumentId);
        var ocrText = ocrResults.OrderByDescending(r => r.ProcessedDate).FirstOrDefault()?.RawText ?? string.Empty;

        // 3. Çıkarılan alanları getir
        var extractedFields = await _unitOfWork.ExtractedFields.FindAsync(f => f.PdfDocumentId == pdfDocumentId);

        // 4. XML üret
        var xmlContent = _xmlBuilder
            .StartDocument("1.0", DateTime.UtcNow.ToString("o"))
            .AddDocumentInfo(pdf)
            .AddOcrText(ocrText)
            .AddExtractedFields(extractedFields.ToList())
            .Build();

        // 5. Mevcut arşivi güncelle veya yeni oluştur
        var existingArchives = await _unitOfWork.XmlArchives.FindAsync(a => a.PdfDocumentId == pdfDocumentId);
        var existing = existingArchives.FirstOrDefault();

        XmlArchive archive;
        if (existing != null)
        {
            existing.XmlContent = xmlContent;
            existing.CreatedDate = DateTime.UtcNow;
            _unitOfWork.XmlArchives.Update(existing);
            archive = existing;
        }
        else
        {
            archive = new XmlArchive
            {
                PdfDocumentId = pdfDocumentId,
                XmlContent = xmlContent,
                CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.XmlArchives.AddAsync(archive);
        }

        // 6. PDF durumunu güncelle
        pdf.Status = DocumentStatus.XmlCreated.ToString();
        _unitOfWork.PdfDocuments.Update(pdf);

        // 7. Audit log (Log servisinde SaveChangesAsync kaldırıldı, transaction burada commitlenecek)
        await _auditLogService.LogAsync(
            action: DocumentStatus.XmlCreated.ToString(),
            description: $"[PdfId: {pdfDocumentId}] XML arşivi oluşturuldu/güncellendi. Alan sayısı: {extractedFields.Count()}",
            entityType: "PdfDocument",
            entityId: pdfDocumentId.ToString(),
            severity: "Info");

        // 8. Tüm değişiklikleri tek bir transaction içerisinde commit et
        await _unitOfWork.SaveChangesAsync();


        _logger.LogInformation("XML başarıyla üretildi ve kaydedildi. PdfId: {PdfId}", pdfDocumentId);

        return new XmlCreateResultDto
        {
            ArchiveId = archive.Id,
            PdfDocumentId = pdfDocumentId,
            CreatedDate = archive.CreatedDate,
            Message = "XML başarıyla üretildi ve arşivlendi."
        };
    }

    public async Task<XmlArchiveDto?> GetByPdfIdAsync(int pdfDocumentId)
    {
        var archives = await _unitOfWork.XmlArchives.FindAsync(a => a.PdfDocumentId == pdfDocumentId);
        var archive = archives.OrderByDescending(a => a.CreatedDate).FirstOrDefault();
        if (archive == null) return null;

        return new XmlArchiveDto
        {
            Id = archive.Id,
            PdfDocumentId = archive.PdfDocumentId,
            XmlContent = archive.XmlContent,
            CreatedDate = archive.CreatedDate
        };
    }


}
