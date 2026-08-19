using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;

namespace NumuneKabul.Application.Services;

public class FormTemplateService : IFormTemplateService
{
    private readonly IUnitOfWork _unitOfWork;

    public FormTemplateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FormTemplateDto>> GetAllAsync()
    {
        var templates = await _unitOfWork.FormTemplates.GetAllAsync();
        return templates.Select(MapToDto).ToList();
    }

    public async Task<List<FormTemplateDto>> GetByInstitutionIdAsync(int institutionId)
    {
        var templates = await _unitOfWork.FormTemplates.GetByInstitutionIdAsync(institutionId);
        return templates.Select(MapToDto).ToList();
    }

    public async Task<FormTemplateDto?> GetByIdAsync(int id)
    {
        var template = await _unitOfWork.FormTemplates.GetByIdWithFieldsAsync(id);
        if (template == null) return null;
        return MapToDto(template);
    }

    public async Task<int> CreateAsync(CreateFormTemplateDto dto)
    {
        var template = new FormTemplate
        {
            InstitutionId = dto.InstitutionId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            BaseImageWidth = dto.BaseImageWidth,
            BaseImageHeight = dto.BaseImageHeight,
            CreatedDate = DateTime.UtcNow,
            TemplateFields = dto.TemplateFields.Select(f => new TemplateField
            {
                FieldName = f.FieldName,
                Regex = f.Regex,
                Keyword = f.Keyword,
                ValidationRegex = f.ValidationRegex,
                Required = f.Required,
                DataType = f.DataType,
                OrderNo = f.OrderNo,
                X = f.X,
                Y = f.Y,
                Width = f.Width,
                Height = f.Height,
                Psm = f.Psm
            }).ToList()
        };

        await _unitOfWork.FormTemplates.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return template.Id;
    }

    public async Task UpdateAsync(int id, UpdateFormTemplateDto dto)
    {
        var template = await _unitOfWork.FormTemplates.GetByIdWithFieldsAsync(id);
        if (template == null) throw new KeyNotFoundException($"Id={id} olan form şablonu bulunamadı.");

        template.InstitutionId = dto.InstitutionId;
        template.Name = dto.Name;
        template.Description = dto.Description;
        template.IsActive = dto.IsActive;
        template.BaseImageWidth = dto.BaseImageWidth;
        template.BaseImageHeight = dto.BaseImageHeight;

        var updatedFieldIds = dto.TemplateFields.Where(f => f.Id > 0).Select(f => f.Id).ToList();
        var fieldsToRemove = template.TemplateFields.Where(f => !updatedFieldIds.Contains(f.Id)).ToList();
        foreach (var field in fieldsToRemove)
        {
            _unitOfWork.TemplateFields.Delete(field);
        }

        foreach (var fieldDto in dto.TemplateFields)
        {
            if (fieldDto.Id == 0)
            {
                template.TemplateFields.Add(new TemplateField
                {
                    FieldName = fieldDto.FieldName,
                    Regex = fieldDto.Regex,
                    Keyword = fieldDto.Keyword,
                    ValidationRegex = fieldDto.ValidationRegex,
                    Required = fieldDto.Required,
                    DataType = fieldDto.DataType,
                    OrderNo = fieldDto.OrderNo,
                    X = fieldDto.X,
                    Y = fieldDto.Y,
                    Width = fieldDto.Width,
                    Height = fieldDto.Height,
                    Psm = fieldDto.Psm
                });
            }
            else
            {
                var existingField = template.TemplateFields.FirstOrDefault(f => f.Id == fieldDto.Id);
                if (existingField != null)
                {
                    existingField.FieldName = fieldDto.FieldName;
                    existingField.Regex = fieldDto.Regex;
                    existingField.Keyword = fieldDto.Keyword;
                    existingField.ValidationRegex = fieldDto.ValidationRegex;
                    existingField.Required = fieldDto.Required;
                    existingField.DataType = fieldDto.DataType;
                    existingField.OrderNo = fieldDto.OrderNo;
                    existingField.X = fieldDto.X;
                    existingField.Y = fieldDto.Y;
                    existingField.Width = fieldDto.Width;
                    existingField.Height = fieldDto.Height;
                    existingField.Psm = fieldDto.Psm;
                }
            }
        }

        _unitOfWork.FormTemplates.Update(template);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var template = await _unitOfWork.FormTemplates.GetByIdWithFieldsAsync(id);
        if (template != null)
        {
            _unitOfWork.FormTemplates.Delete(template);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private FormTemplateDto MapToDto(FormTemplate template)
    {
        return new FormTemplateDto
        {
            Id = template.Id,
            InstitutionId = template.InstitutionId,
            InstitutionName = template.Institution?.Name ?? string.Empty,
            Name = template.Name,
            Description = template.Description,
            IsActive = template.IsActive,
            BaseImageWidth = template.BaseImageWidth,
            BaseImageHeight = template.BaseImageHeight,
            TemplateFields = template.TemplateFields.Select(f => new TemplateFieldDto
            {
                Id = f.Id,
                TemplateId = f.TemplateId,
                FieldName = f.FieldName,
                Regex = f.Regex,
                Keyword = f.Keyword,
                ValidationRegex = f.ValidationRegex,
                Required = f.Required,
                DataType = f.DataType,
                OrderNo = f.OrderNo,
                X = f.X,
                Y = f.Y,
                Width = f.Width,
                Height = f.Height,
                Psm = f.Psm
            }).OrderBy(f => f.OrderNo).ToList()
        };
    }
}
