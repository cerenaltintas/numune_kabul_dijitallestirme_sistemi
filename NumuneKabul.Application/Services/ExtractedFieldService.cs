using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Enums;
using NumuneKabul.Domain.Interfaces;

namespace NumuneKabul.Application.Services;

public class ExtractedFieldService : IExtractedFieldService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public ExtractedFieldService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task SaveResultsAsync(int pdfDocumentId, List<ExtractedResultDto> results)
    {
        // Önce mevcut sonuçları temizle
        var existing = await _unitOfWork.ExtractedFields.FindAsync(e => e.PdfDocumentId == pdfDocumentId);
        foreach (var item in existing)
        {
            _unitOfWork.ExtractedFields.Delete(item);
        }

        // Yeni sonuçları ekle
        foreach (var result in results)
        {
            var entity = new ExtractedField
            {
                PdfDocumentId = pdfDocumentId,
                FieldName = result.FieldName,
                RawValue = result.RawValue,
                Confidence = result.Confidence,
                PageNo = result.PageNo,
                X = result.X,
                Y = result.Y,
                Width = result.Width,
                Height = result.Height,
                Status = result.RawValue != null ? "Extracted" : "Missing"
            };
            await _unitOfWork.ExtractedFields.AddAsync(entity);

        }

        await _unitOfWork.SaveChangesAsync();

        // Audit Log yazımı
        // Toplu alan özeti: her alan için tek tek log yerine özet bir kayıt
        var extracted = results.Count(r => r.RawValue != null);
        var missing = results.Count(r => r.RawValue == null);
        await _auditLogService.LogAsync(
            action: "RegexExtraction",
            description: $"[PdfId: {pdfDocumentId}] Çıkarma tamamlandı. Bulunan: {extracted}, Bulunamayan: {missing}",
            entityType: "PdfDocument",
            entityId: pdfDocumentId.ToString(),
            severity: missing > 0 ? "Warning" : "Info");

        return;

    }

    public async Task<List<ExtractedResultDto>> GetByPdfIdAsync(int pdfDocumentId)
    {
        var fields = await _unitOfWork.ExtractedFields.FindAsync(e => e.PdfDocumentId == pdfDocumentId);
        return fields.Select(e => new ExtractedResultDto
        {
            Id = e.Id,
            FieldName = e.FieldName,
            RawValue = e.Status == "Corrected" ? (e.CorrectedValue ?? "") : (e.CorrectedValue ?? e.RawValue),
            Confidence = e.Confidence,
            PageNo = e.PageNo,
            X = e.X,
            Y = e.Y,
            Width = e.Width,
            Height = e.Height,
            Status = e.Status,
            Severity = e.Status == "Corrected" ? "Success" : 
                       (string.IsNullOrWhiteSpace(e.RawValue) || e.RawValue == "(boş)" ? "Danger" : 
                       (e.Confidence < 0.70m ? "Warning" : "Success"))
        }).ToList();
    }

    /// <summary>
    /// Kullanıcı manuel düzeltme yapar → her değişiklik Audit Log'a yazılır.
    /// </summary>
    public async Task<bool> UpdateFieldAsync(int fieldId, UpdateExtractedFieldDto dto, int userId)
    {
        var fields = await _unitOfWork.ExtractedFields.FindAsync(f => f.Id == fieldId);
        var field = fields.FirstOrDefault();
        
        if (field == null) return false;

        var oldValue = field.CorrectedValue ?? field.RawValue ?? "(boş)";

        field.CorrectedValue = dto.CorrectedValue ?? "";
        field.Status = DocumentStatus.Corrected.ToString();
        _unitOfWork.ExtractedFields.Update(field);

        await _unitOfWork.SaveChangesAsync();

        // Audit Log yazımı
        // "Her değişiklik Audit Log'a yazılacaktır."
        var oldValuesJson = System.Text.Json.JsonSerializer.Serialize(new { RawValue = oldValue });
        var newValuesJson = System.Text.Json.JsonSerializer.Serialize(new { CorrectedValue = dto.CorrectedValue, Notes = dto.Notes });

        await _auditLogService.LogAsync(
            action: "ManualFieldCorrection",
            description: $"[FieldId: {fieldId}, Alan: {field.FieldName}] Manuel düzeltme yapıldı.",
            entityType: "PdfDocument",
            entityId: field.PdfDocumentId.ToString(),
            severity: "Info",
            oldValues: oldValuesJson,
            newValues: newValuesJson);

        // Belge statüsü güncellemesi (NeedsManualReview ise Corrected'a çevir)
        var pdf = await _unitOfWork.PdfDocuments.GetByIdAsync(field.PdfDocumentId);
        if (pdf != null && pdf.Status == DocumentStatus.NeedsManualReview.ToString())
        {
            pdf.Status = DocumentStatus.Corrected.ToString();
            _unitOfWork.PdfDocuments.Update(pdf);
            await _unitOfWork.SaveChangesAsync();
        }

        return true;
    }

    /// <summary>
    /// Toplu alan düzeltme — birden fazla alanı tek seferde günceller.
    /// </summary>
    public async Task SaveCorrectionsAsync(int pdfDocumentId, List<UpdateExtractedFieldDto> corrections, int userId)
    {
        foreach (var correction in corrections)
        {
            await UpdateFieldAsync(correction.Id, correction, userId);
        }
    }
}
