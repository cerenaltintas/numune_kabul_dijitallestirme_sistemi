using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Web.Filters;
using NumuneKabul.Web.Services;

namespace NumuneKabul.Web.Controllers;

[SessionAuthorize(Roles = "Admin")]
public class InstitutionController : Controller
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<InstitutionController> _logger;

    public InstitutionController(IApiClientService apiClient, ILogger<InstitutionController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var institutions = await _apiClient.GetAllInstitutionsAsync();
        return View(institutions);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateInstitutionDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInstitutionDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var success = await _apiClient.CreateInstitutionAsync(model);
        if (success)
        {
            TempData["SuccessMessage"] = "Kurum başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Kurum eklenirken bir hata oluştu.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var institution = await _apiClient.GetInstitutionAsync(id);
        if (institution == null) return NotFound();

        return View(new UpdateInstitutionDto { Id = institution.Id, Name = institution.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateInstitutionDto model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var success = await _apiClient.UpdateInstitutionAsync(id, model);
        if (success)
        {
            TempData["SuccessMessage"] = "Kurum başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Kurum güncellenirken bir hata oluştu.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _apiClient.DeleteInstitutionAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Kurum başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Kurum silinirken bir hata oluştu. Kuruma bağlı kullanıcılar olabilir.";
        }
        return RedirectToAction(nameof(Index));
    }
}
