using AutoMapper;
using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Interfaces;
using System.Linq;

namespace NumuneKabul.Application.Services;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LookupService> _logger;

    public LookupService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LookupService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<InstitutionDto>> GetInstitutionsAsync()
    {
        var institutions = await _unitOfWork.Institutions.GetAllAsync();
        return _mapper.Map<IEnumerable<InstitutionDto>>(institutions);
    }

    public async Task<IEnumerable<FormTemplateDto>> GetTemplatesByInstitutionAsync(int institutionId)
    {
        // Tüm listeyi RAM'e çekmemek için veritabanı seviyesinde filtreleme yapıyoruz.
        var filtered = await _unitOfWork.FormTemplates.FindAsync(t => t.InstitutionId == institutionId);
        
        return _mapper.Map<IEnumerable<FormTemplateDto>>(filtered);
    }
}
