using Microsoft.EntityFrameworkCore;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using NumuneKabul.Infrastructure.Data;

namespace NumuneKabul.Infrastructure.Repositories;

public class FormTemplateRepository : GenericRepository<FormTemplate>, IFormTemplateRepository
{
    public FormTemplateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<FormTemplate>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.Institution)
            .Include(t => t.TemplateFields)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<FormTemplate>> GetByInstitutionIdAsync(int institutionId)
    {
        return await _dbSet
            .Include(t => t.Institution)
            .Include(t => t.TemplateFields)
            .Where(t => t.InstitutionId == institutionId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<FormTemplate?> GetByIdWithFieldsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Institution)
            .Include(t => t.TemplateFields)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
