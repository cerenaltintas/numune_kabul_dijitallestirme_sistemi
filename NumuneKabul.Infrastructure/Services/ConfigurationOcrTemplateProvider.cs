using Microsoft.Extensions.Configuration;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services;

/// <summary>
/// Ocr Şablonlarını IConfiguration (appsettings.json) üzerinden okuyan somut sınıf.
/// Yarın şablonları veritabanından çekmek istersek, 
/// sadece bu interface'in yeni bir implementasyonunu yazacağız.
/// </summary>
public class ConfigurationOcrTemplateProvider : IOcrTemplateProvider
{
    private readonly IConfiguration _configuration;

    public ConfigurationOcrTemplateProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public OcrTemplateDto? GetTemplate(string templateName)
    {
        var templates = _configuration.GetSection("OcrTemplates").Get<List<OcrTemplateDto>>();
        if (templates == null || templates.Count == 0)
            return null;

        return templates.FirstOrDefault(t => string.Equals(t.TemplateName, templateName, StringComparison.OrdinalIgnoreCase));
    }

    public OcrTemplateDto? GetTemplateById(int id)
    {
        // appsettings.json şablonlarında ID mantığı olmadığı için doğrudan null dönebiliriz.
        // Zaten bu sınıf şu an Dependency Injection'dan çıkarıldı, Database provider kullanılıyor.
        return null;
    }

    public OcrTemplateDto? GetDefaultTemplate()
    {
        var templates = _configuration.GetSection("OcrTemplates").Get<List<OcrTemplateDto>>();
        
        // Eğer hiçbir şablon tanımlanmamışsa null döner
        if (templates == null || templates.Count == 0)
            return null;

        // Varsayılan olarak konfigürasyondaki ilk şablonu (veya adı Bilkent olanı) döner
        return templates.FirstOrDefault(t => t.TemplateName == "Bilkent_Patoloji_Formu") ?? templates.First();
    }
}
