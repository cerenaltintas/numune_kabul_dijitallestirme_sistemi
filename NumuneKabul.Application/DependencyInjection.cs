using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Application.Services;
using NumuneKabul.Application.Services.XmlBuilders;
using NumuneKabul.Application.Services.XmlMappers;

namespace NumuneKabul.Application;

//Interface-Implementation eşleşmelerinin ve AutoMapper/FluentValidation konfigürasyonlarının DI (Dependency Injection) mekanizmasına entegre edildiği yer.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IFormTemplateService, FormTemplateService>();
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
        services.AddScoped<IExtractedFieldService, ExtractedFieldService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddScoped<IXmlBuilder, StandardXmlBuilder>();
        
        // XML Mapping Strategies
        services.AddScoped<IXmlMapperStrategy, MockRestXmlMapper>();
        services.AddScoped<IXmlMappingService, XmlMappingService>();

        services.AddScoped<IXmlService, XmlService>();
        services.AddScoped<IIntegrationService, IntegrationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
