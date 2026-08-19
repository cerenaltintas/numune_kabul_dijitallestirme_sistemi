using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NumuneKabul.Application.Services;

public class InstitutionService : IInstitutionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InstitutionService> _logger;

    public InstitutionService(IUnitOfWork unitOfWork, ILogger<InstitutionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<InstitutionDto>> GetAllInstitutionsAsync()
    {
        var institutions = await _unitOfWork.Institutions.GetAllAsync();
        return institutions.Select(i => new InstitutionDto { Id = i.Id, Name = i.Name }).ToList();
    }

    public async Task<InstitutionDto?> GetInstitutionByIdAsync(int id)
    {
        var institution = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (institution == null) return null;

        return new InstitutionDto { Id = institution.Id, Name = institution.Name };
    }

    public async Task<InstitutionDto> CreateInstitutionAsync(CreateInstitutionDto dto)
    {
        var institution = new Institution { Name = dto.Name };
        await _unitOfWork.Institutions.AddAsync(institution);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Yeni kurum eklendi. ID: {Id}, Adı: {Name}", institution.Id, institution.Name);

        return new InstitutionDto { Id = institution.Id, Name = institution.Name };
    }

    public async Task UpdateInstitutionAsync(int id, UpdateInstitutionDto dto)
    {
        var institution = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (institution == null) 
            throw new KeyNotFoundException($"Id={id} olan kurum bulunamadı.");

        institution.Name = dto.Name;
        _unitOfWork.Institutions.Update(institution);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Kurum güncellendi. ID: {Id}", institution.Id);
    }

    public async Task DeleteInstitutionAsync(int id)
    {
        var institution = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (institution == null) 
            throw new KeyNotFoundException($"Id={id} olan kurum bulunamadı.");

        // Check if there are users or templates attached
        var hasUsers = (await _unitOfWork.Users.FindAsync(u => u.InstitutionId == id)).Any();
        if (hasUsers) 
            throw new InvalidOperationException("Bu kuruma bağlı kullanıcılar olduğu için silinemez.");

        _unitOfWork.Institutions.Delete(institution);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Kurum silindi. ID: {Id}", id);
    }
}
