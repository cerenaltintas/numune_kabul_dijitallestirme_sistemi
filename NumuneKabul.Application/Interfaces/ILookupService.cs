using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Arayüzdeki açılır listeleri doldurmak için kullanılan, read-only referans veri çekme sözleşmesi.
/// </summary>
public interface ILookupService
{
    Task<IEnumerable<InstitutionDto>> GetInstitutionsAsync();
    Task<IEnumerable<FormTemplateDto>> GetTemplatesByInstitutionAsync(int institutionId);
}
