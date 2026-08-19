using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

// Kurumlara ait OCR şablonlarının ve koordinat haritalarının CRUD işlemlerini yöneten servis sözleşmesi.
public interface IFormTemplateService
{
    Task<List<FormTemplateDto>> GetAllAsync();
    Task<List<FormTemplateDto>> GetByInstitutionIdAsync(int institutionId);
    Task<FormTemplateDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateFormTemplateDto dto);
    Task UpdateAsync(int id, UpdateFormTemplateDto dto);
    Task DeleteAsync(int id);
}
