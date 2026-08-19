using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using NumuneKabul.Infrastructure.Data;
using NumuneKabul.Infrastructure.Repositories;

namespace NumuneKabul.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    private IGenericRepository<User>? _users;
    private IGenericRepository<Institution>? _institutions;
    private IFormTemplateRepository? _formTemplates;
    private IGenericRepository<TemplateField>? _templateFields;
    private IPdfDocumentRepository? _pdfDocuments;
    private IGenericRepository<OcrResult>? _ocrResults;
    private IGenericRepository<ExtractedField>? _extractedFields;
    private IGenericRepository<XmlArchive>? _xmlArchives;
    private IGenericRepository<IntegrationJob>? _integrationJobs;
    private IGenericRepository<AuditLog>? _auditLogs;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<Institution> Institutions => _institutions ??= new GenericRepository<Institution>(_context);
    public IFormTemplateRepository FormTemplates => _formTemplates ??= new FormTemplateRepository(_context);
    public IGenericRepository<TemplateField> TemplateFields => _templateFields ??= new GenericRepository<TemplateField>(_context);
    public IPdfDocumentRepository PdfDocuments => _pdfDocuments ??= new PdfDocumentRepository(_context);
    public IGenericRepository<OcrResult> OcrResults => _ocrResults ??= new GenericRepository<OcrResult>(_context);
    public IGenericRepository<ExtractedField> ExtractedFields => _extractedFields ??= new GenericRepository<ExtractedField>(_context);
    public IGenericRepository<XmlArchive> XmlArchives => _xmlArchives ??= new GenericRepository<XmlArchive>(_context);
    public IGenericRepository<IntegrationJob> IntegrationJobs => _integrationJobs ??= new GenericRepository<IntegrationJob>(_context);
    public IGenericRepository<AuditLog> AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
