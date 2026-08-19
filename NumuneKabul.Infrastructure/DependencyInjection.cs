using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Interfaces;
using NumuneKabul.Infrastructure.Data;
using NumuneKabul.Infrastructure.Repositories;
using NumuneKabul.Infrastructure.Services;
using NumuneKabul.Infrastructure.Services.Adapters;

namespace NumuneKabul.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
        var connectionString = configuration.GetConnectionString(provider);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (provider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
                options.UseSqlServer(connectionString);
            else
                options.UseSqlite(connectionString);
        });

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IPdfDocumentRepository, PdfDocumentRepository>();
        services.AddScoped<IFieldValidatorService, FieldValidatorService>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        
        // Adapters
        services.AddHttpClient<IIntegrationAdapter, MockRestIntegrationAdapter>((provider, client) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var mockUrl = config["IntegrationSettings:MockRestUrl"] ?? "https://jsonplaceholder.typicode.com/posts";
            client.BaseAddress = new Uri(mockUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        
        // OCR Servisi
        services.AddScoped<NumuneKabul.Application.Interfaces.IOcrTemplateProvider, NumuneKabul.Infrastructure.Services.DatabaseOcrTemplateProvider>();
        services.AddScoped<NumuneKabul.Application.Interfaces.IOcrService, NumuneKabul.Infrastructure.Services.TesseractOcrService>();

        services.AddScoped<NumuneKabul.Application.Interfaces.IPdfImageService, NumuneKabul.Infrastructure.Services.PdfImageService>();
        services.AddScoped<NumuneKabul.Application.Interfaces.IFileStorageService, NumuneKabul.Infrastructure.Services.LocalFileStorageService>();

        services.AddScoped<NumuneKabul.Application.Interfaces.IExtractionStrategy, NumuneKabul.Infrastructure.Services.ExtractionStrategies.ZonalExtractionStrategy>();
        services.AddScoped<NumuneKabul.Application.Interfaces.IExtractionStrategy, NumuneKabul.Infrastructure.Services.ExtractionStrategies.RegexExtractionStrategy>();
        services.AddScoped<NumuneKabul.Application.Interfaces.IExtractionStrategy, NumuneKabul.Infrastructure.Services.ExtractionStrategies.KeywordExtractionStrategy>();
        services.AddScoped<NumuneKabul.Application.Interfaces.IExtractionEngine, NumuneKabul.Infrastructure.Services.SmartExtractionEngine>();
        services.AddScoped<NumuneKabul.Application.Interfaces.ICoordinateMapperService, NumuneKabul.Infrastructure.Services.CoordinateMapperService>();
        services.AddScoped<NumuneKabul.Application.Interfaces.IDocumentConfidenceScorer, NumuneKabul.Infrastructure.Services.DocumentConfidenceScorer>();
        services.AddScoped<NumuneKabul.Application.Interfaces.IFieldValidatorService, NumuneKabul.Infrastructure.Services.FieldValidatorService>();

        return services;
    }
}
