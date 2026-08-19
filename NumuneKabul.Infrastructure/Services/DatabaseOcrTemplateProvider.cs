using Microsoft.EntityFrameworkCore;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Infrastructure.Data;

namespace NumuneKabul.Infrastructure.Services;

/// <summary>
/// Ocr Şablonlarını veritabanı üzerinden (ApplicationDbContext) okuyan sınıf.
/// </summary>
public class DatabaseOcrTemplateProvider : IOcrTemplateProvider
{
    private readonly ApplicationDbContext _context;

    public DatabaseOcrTemplateProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public OcrTemplateDto? GetTemplate(string templateName)
    {
        var template = _context.FormTemplates
            .Include(t => t.TemplateFields)
            .FirstOrDefault(t => t.Name == templateName && t.IsActive);

        if (template == null) return null;

        return MapToOcrTemplateDto(template);
    }

    public OcrTemplateDto? GetTemplateById(int id)
    {
        var template = _context.FormTemplates
            .Include(t => t.TemplateFields)
            .FirstOrDefault(t => t.Id == id && t.IsActive);

        if (template == null) return null;

        return MapToOcrTemplateDto(template);
    }

    public OcrTemplateDto? GetDefaultTemplate()
    {
        var template = _context.FormTemplates
            .Include(t => t.TemplateFields)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Id)
            .FirstOrDefault();

        if (template == null) return null;

        return MapToOcrTemplateDto(template);
    }

    private OcrTemplateDto MapToOcrTemplateDto(Domain.Entities.FormTemplate template)
    {
        return new OcrTemplateDto
        {
            TemplateName = template.Name,
            BaseImageWidth = template.BaseImageWidth,
            BaseImageHeight = template.BaseImageHeight,
            Zones = template.TemplateFields
                .Where(f => f.X.HasValue && f.Y.HasValue && f.Width.HasValue && f.Height.HasValue)
                .Select(f => new OcrZoneDto
                {
                    Key = f.FieldName,
                    X = f.X.Value,
                    Y = f.Y.Value,
                    Width = f.Width.Value,
                    Height = f.Height.Value,
                    Psm = f.Psm
                }).ToList()
        };
    }
}
