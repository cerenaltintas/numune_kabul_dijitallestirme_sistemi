using AutoMapper;
using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Enums;
using System.Text.Json;
using NumuneKabul.Domain.Interfaces;

namespace NumuneKabul.Application.Services;

public class PdfService : IPdfService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PdfService> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogService _auditLogService;
    private const string PdfUploadFolder = "uploads/pdfs";

    public PdfService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PdfService> logger, IFileStorageService fileStorageService, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _fileStorageService = fileStorageService;
        _auditLogService = auditLogService;
    }

    public async Task<PdfUploadResultDto> UploadPdfAsync(Stream fileStream, string fileName, int? institutionId, int? templateId)
    {
        // 1. Şablon - Kurum uyuşmazlığı doğrulaması
        if (templateId.HasValue && institutionId.HasValue)
        {
            var template = await _unitOfWork.FormTemplates.GetByIdAsync(templateId.Value);
            if (template == null || template.InstitutionId != institutionId.Value)
            {
                _logger.LogWarning("Geçersiz şablon seçimi. TemplateId: {TemplateId}, InstitutionId: {InstitutionId}", templateId, institutionId);
                throw new ArgumentException("Seçilen şablon bu kuruma ait değildir veya bulunamadı.");
            }
        }

        // 2. IFileStorageService kullanarak dosyayı kaydet
        var relativeFilePath = await _fileStorageService.SaveFileAsync(fileStream, fileName, PdfUploadFolder);

        try
        {
            // 3. Veritabanına kaydet
            var pdfDocument = new PdfDocument
            {
                InstitutionId = institutionId,
                TemplateId = templateId,
                FileName = Path.GetFileName(fileName),
                FilePath = relativeFilePath,
                UploadDate = DateTime.UtcNow,
                Status = DocumentStatus.Uploaded.ToString(),
                PageCount = 0
            };

            await _unitOfWork.PdfDocuments.AddAsync(pdfDocument);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("PDF yüklendi: {FileName}, Id: {Id}", pdfDocument.FileName, pdfDocument.Id);

            // İşlem takibi için Audit Log
            await _auditLogService.LogAsync(
                action: "PDF_Uploaded",
                description: $"Sisteme yeni PDF yüklendi. Dosya Adı: {pdfDocument.FileName}",
                entityType: "PdfDocument",
                entityId: pdfDocument.Id.ToString(),
                severity: "Info",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    pdfDocument.FileName, 
                    pdfDocument.InstitutionId, 
                    pdfDocument.TemplateId 
                })
            );

            return _mapper.Map<PdfUploadResultDto>(pdfDocument);
        }
        catch (Exception ex)
        {
            // Veritabanı kaydı başarısız olursa, fiziksel dosyayı sil (Rollback)
            await _fileStorageService.DeleteFileAsync(relativeFilePath);
            _logger.LogWarning(ex, "Veritabanı kaydı başarısız olduğu için fiziksel dosya silindi: {FilePath}", relativeFilePath);
            throw;
        }
    }

    public async Task<PdfDocumentDto?> GetPdfByIdAsync(int id)
    {
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(id);
        return pdf == null ? null : _mapper.Map<PdfDocumentDto>(pdf);
    }

    public async Task<IEnumerable<PdfDocumentDto>> GetAllPdfsAsync()
    {
        var pdfs = await _unitOfWork.PdfDocuments.GetAllAsync();
        return _mapper.Map<IEnumerable<PdfDocumentDto>>(pdfs);
    }

    public async Task<IEnumerable<PdfDocumentDto>> GetPdfsByInstitutionAsync(int institutionId)
    {
        var pdfs = await _unitOfWork.PdfDocuments.GetByInstitutionIdAsync(institutionId);
        return _mapper.Map<IEnumerable<PdfDocumentDto>>(pdfs);
    }

    public async Task<IEnumerable<PdfDocumentDto>> GetPdfsByStatusAsync(string status)
    {
        var pdfs = await _unitOfWork.PdfDocuments.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<PdfDocumentDto>>(pdfs);
    }

    public async Task<PaginatedResult<PdfDocumentDto>> GetPdfsPaginatedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _unitOfWork.PdfDocuments.GetPaginatedAsync(page, pageSize);
        return new PaginatedResult<PdfDocumentDto>
        {
            Items = _mapper.Map<IEnumerable<PdfDocumentDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<PdfDocumentDto>> GetPdfsPaginatedByInstitutionAsync(int page, int pageSize, int institutionId)
    {
        var (items, totalCount) = await _unitOfWork.PdfDocuments.GetPaginatedByInstitutionAsync(page, pageSize, institutionId);
        return new PaginatedResult<PdfDocumentDto>
        {
            Items = _mapper.Map<IEnumerable<PdfDocumentDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<(Stream FileStream, string FileName)?> DownloadPdfAsync(int id)
    {
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(id);
        if (pdf == null) return null;

        var fileStream = await _fileStorageService.GetFileStreamAsync(pdf.FilePath);
        if (fileStream == null) return null;

        return (fileStream, pdf.FileName);
    }

    public async Task<bool> DeletePdfAsync(int id)
    {
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(id);
        if (pdf == null)
            return false;

        // ÖNCE veritabanı kaydını siliyoruz ki tutarsızlık olmasın
        _unitOfWork.PdfDocuments.Delete(pdf);
        await _unitOfWork.SaveChangesAsync();

        // Veritabanı başarılı olursa fiziksel dosyayı siliyoruz
        await _fileStorageService.DeleteFileAsync(pdf.FilePath);

        _logger.LogInformation("PDF kaydı silindi: Id={Id}, FileName={FileName}", id, pdf.FileName);
        return true;
    }

    public async Task SaveOcrResultAsync(int pdfId, OcrEngineResultDto result)
    {
        var rawWordsJson = JsonSerializer.Serialize(result.Words);

        // 1. Varsa eski OcrResult nesnesini buluyoruz
        var existingResults = await _unitOfWork.OcrResults.FindAsync(x => x.PdfDocumentId == pdfId);
        var existing = existingResults.OrderByDescending(x => x.ProcessedDate).FirstOrDefault();

        if (existing != null)
        {
            existing.RawText = result.Text;
            existing.RawWordsJson = rawWordsJson;
            existing.AverageConfidence = result.AverageConfidence;
            existing.ProcessedDate = DateTime.UtcNow;
            _unitOfWork.OcrResults.Update(existing);
        }
        else
        {
            var ocrResult = new NumuneKabul.Domain.Entities.OcrResult
            {
                PdfDocumentId = pdfId,
                RawText = result.Text,
                RawWordsJson = rawWordsJson,
                AverageConfidence = result.AverageConfidence,
                ProcessedDate = DateTime.UtcNow
            };
            await _unitOfWork.OcrResults.AddAsync(ocrResult);
        }
        
        // 3. Belgenin Durumunu "OCR Tamamlandı" olarak güncelliyoruz
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(pdfId);
        if (pdf != null)
        {
            pdf.Status = DocumentStatus.OcrCompleted.ToString();
            _unitOfWork.PdfDocuments.Update(pdf);
        }
        
        // 4. Değişiklikleri kalıcı olarak kaydetmesini (Save) söylüyoruz
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("OCR sonucu veritabanına başarıyla kaydedildi/güncellendi. PdfId: {PdfId}", pdfId);
    }

    public async Task<OcrEngineResultDto?> GetSavedOcrDataAsync(int pdfId)
    {
        var results = await _unitOfWork.OcrResults.FindAsync(x => x.PdfDocumentId == pdfId);
        var latest = results.OrderByDescending(x => x.ProcessedDate).FirstOrDefault();
        
        if (latest == null) return null;

        var words = !string.IsNullOrEmpty(latest.RawWordsJson) 
            ? JsonSerializer.Deserialize<List<OcrWordDto>>(latest.RawWordsJson) ?? new List<OcrWordDto>()
            : new List<OcrWordDto>();

        return new OcrEngineResultDto
        {
            Text = latest.RawText,
            AverageConfidence = latest.AverageConfidence,
            Words = words
        };
    }

    public async Task UpdateTemplateAsync(int pdfId, int institutionId, int templateId)
    {
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(pdfId);
        if (pdf == null) throw new KeyNotFoundException("PDF bulunamadı.");

        pdf.InstitutionId = institutionId;
        pdf.TemplateId = templateId;
        
        _unitOfWork.PdfDocuments.Update(pdf);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateConfidenceScoreAsync(int pdfId, decimal score)
    {
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(pdfId);
        if (pdf != null)
        {
            pdf.ConfidenceScore = score;
            
            // Eğer skor belirli bir eşiğin altındaysa (örnek: 70), NeedsManualReview statüsüne al
            if (score < 70)
            {
                pdf.Status = DocumentStatus.NeedsManualReview.ToString();
            }
            
            _unitOfWork.PdfDocuments.Update(pdf);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
