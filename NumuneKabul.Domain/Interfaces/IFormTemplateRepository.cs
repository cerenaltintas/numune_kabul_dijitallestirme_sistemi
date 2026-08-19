using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Domain.Interfaces;

public interface IFormTemplateRepository : IGenericRepository<FormTemplate>
{
    Task<IEnumerable<FormTemplate>> GetByInstitutionIdAsync(int institutionId);
    Task<FormTemplate?> GetByIdWithFieldsAsync(int id);
}
