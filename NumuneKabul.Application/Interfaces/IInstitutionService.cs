using NumuneKabul.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NumuneKabul.Application.Interfaces;

public interface IInstitutionService
{
    Task<List<InstitutionDto>> GetAllInstitutionsAsync();
    Task<InstitutionDto?> GetInstitutionByIdAsync(int id);
    Task<InstitutionDto> CreateInstitutionAsync(CreateInstitutionDto dto);
    Task UpdateInstitutionAsync(int id, UpdateInstitutionDto dto);
    Task DeleteInstitutionAsync(int id);
}
