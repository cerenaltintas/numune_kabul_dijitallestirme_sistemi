using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Domain.Interfaces;

// tüm depoları tek merkezde toplar ve değişiklikleri tek seferde kaydeder 
public interface IUnitOfWork : IDisposable 
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Institution> Institutions { get; }
    IFormTemplateRepository FormTemplates { get; }
    IGenericRepository<TemplateField> TemplateFields { get; }
    IPdfDocumentRepository PdfDocuments { get; }
    IGenericRepository<OcrResult> OcrResults { get; }
    IGenericRepository<ExtractedField> ExtractedFields { get; }
    IGenericRepository<XmlArchive> XmlArchives { get; }
    IGenericRepository<IntegrationJob> IntegrationJobs { get; }
    IGenericRepository<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync();
}
