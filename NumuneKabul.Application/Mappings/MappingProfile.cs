using AutoMapper;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Application.Mappings;

public class MappingProfile : Profile
{
    //Entities ile DTO'lar arasındaki otomatik dönüştürme/kopyalama kuralları
    public MappingProfile()
    {
        CreateMap<PdfDocument, PdfDocumentDto>()
            .ForMember(d => d.InstitutionName, opt => opt.MapFrom(s => s.Institution != null ? s.Institution.Name : string.Empty))
            .ForMember(d => d.TemplateName, opt => opt.MapFrom(s => s.FormTemplate != null ? s.FormTemplate.Name : null));

        CreateMap<PdfDocument, PdfUploadResultDto>();
        CreateMap<Institution, InstitutionDto>();
        CreateMap<FormTemplate, FormTemplateDto>();
    }
}
