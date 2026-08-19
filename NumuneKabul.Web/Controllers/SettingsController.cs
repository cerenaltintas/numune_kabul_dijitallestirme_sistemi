using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Web.Filters;
using NumuneKabul.Web.Services;

namespace NumuneKabul.Web.Controllers;

[SessionAuthorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(IApiClientService apiClient, ILogger<SettingsController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var settings = await _apiClient.GetSettingsAsync();
        if (settings == null)
        {
            settings = new SettingsDto(); // Default empty if API fails
            TempData["ErrorMessage"] = "Ayarlar API'den alýnamadý.";
        }

        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var success = await _apiClient.UpdateSettingsAsync(model);
        if (success)
        {
            TempData["SuccessMessage"] = "Sistem ayarlarý baþarýyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Ayarlar güncellenirken bir hata oluþtu.");
        return View(model);
    }
}
