using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Web.Filters;
using NumuneKabul.Web.Services;

namespace NumuneKabul.Web.Controllers;

[SessionAuthorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<UserController> _logger;

    public UserController(IApiClientService apiClient, ILogger<UserController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _apiClient.GetUsersAsync();
        return View(users);
    }

    private async Task PopulateInstitutions()
    {
        var institutions = await _apiClient.GetAllInstitutionsAsync();
        ViewBag.Institutions = new SelectList(institutions, "Id", "Name");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateInstitutions();
        return View(new CreateUserDto { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateInstitutions();
            return View(model);
        }

        var success = await _apiClient.CreateUserAsync(model);
        if (success)
        {
            TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Kullanıcı eklenirken bir hata oluştu veya kullanıcı adı zaten kullanımda.");
        await PopulateInstitutions();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _apiClient.GetUserAsync(id);
        if (user == null) return NotFound();

        await PopulateInstitutions();

        return View(new UpdateUserDto 
        { 
            Id = user.Id, 
            Username = user.Username, 
            Name = user.Name, 
            Role = user.Role, 
            InstitutionId = user.InstitutionId,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateUserDto model)
    {
        if (id != model.Id) return BadRequest();
        
        if (!ModelState.IsValid) 
        {
            await PopulateInstitutions();
            return View(model);
        }

        var success = await _apiClient.UpdateUserAsync(id, model);
        if (success)
        {
            TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Kullanıcı güncellenirken bir hata oluştu.");
        await PopulateInstitutions();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _apiClient.DeleteUserAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu.";
        }
        return RedirectToAction(nameof(Index));
    }
}
