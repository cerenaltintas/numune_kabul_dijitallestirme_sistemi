using Microsoft.EntityFrameworkCore;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using NumuneKabul.Infrastructure.Data;

namespace NumuneKabul.Infrastructure.Repositories;

public class PdfDocumentRepository : GenericRepository<PdfDocument>, IPdfDocumentRepository
{
    public PdfDocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<PdfDocument?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Institution)
            .Include(p => p.FormTemplate)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<PdfDocument>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Institution)
            .Include(p => p.FormTemplate)
            .OrderByDescending(p => p.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PdfDocument>> GetByInstitutionIdAsync(int institutionId)
    {
        return await _dbSet
            .Include(p => p.Institution)
            .Include(p => p.FormTemplate)
            .Where(p => p.InstitutionId == institutionId)
            .OrderByDescending(p => p.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PdfDocument>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(p => p.Institution)
            .Include(p => p.FormTemplate)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.UploadDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<PdfDocument> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
    {
        var query = _dbSet
            .Include(p => p.Institution)
            .Include(p => p.FormTemplate);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(p => p.UploadDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<PdfDocument> Items, int TotalCount)> GetPaginatedByInstitutionAsync(int page, int pageSize, int institutionId)
    {
        var query = _dbSet
            .Include(p => p.Institution)
            .Include(p => p.FormTemplate)
            .Where(p => p.InstitutionId == institutionId);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(p => p.UploadDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
