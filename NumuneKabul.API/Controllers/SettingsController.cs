using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public SettingsController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    [HttpGet]
    public IActionResult GetSettings()
    {
        var settings = new SettingsDto
        {
            MockRestUrl = _configuration["IntegrationSettings:MockRestUrl"] ?? "",
            PdfPath = _configuration["StorageSettings:PdfPath"] ?? "",
            ImagePath = _configuration["StorageSettings:ImagePath"] ?? ""
        };

        return Ok(settings);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto dto)
    {
        try
        {
            var filePath = Path.Combine(_env.ContentRootPath, "appsettings.json");
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            var jsonObj = JsonNode.Parse(json)!.AsObject();

            if (jsonObj["IntegrationSettings"] == null) jsonObj["IntegrationSettings"] = new JsonObject();
            jsonObj["IntegrationSettings"]!["MockRestUrl"] = dto.MockRestUrl;

            if (jsonObj["StorageSettings"] == null) jsonObj["StorageSettings"] = new JsonObject();
            jsonObj["StorageSettings"]!["PdfPath"] = dto.PdfPath;
            jsonObj["StorageSettings"]!["ImagePath"] = dto.ImagePath;

            var options = new JsonSerializerOptions { WriteIndented = true };
            await System.IO.File.WriteAllTextAsync(filePath, jsonObj.ToJsonString(options));

            // Reload configuration (might not affect all injected services immediately depending on IOptions implementation, but good enough for this PoC)
            if (_configuration is IConfigurationRoot configRoot)
            {
                configRoot.Reload();
            }

            return Ok(new { Message = "Ayarlar baþarýyla güncellendi." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ayarlar güncellenirken bir hata oluþtu: " + ex.Message);
        }
    }
}
