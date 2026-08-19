using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Web.Models;
using NumuneKabul.Web.Services;

using NumuneKabul.Web.Filters;

namespace NumuneKabul.Web.Controllers;

[SessionAuthorize(Roles = "Admin")]
public class TemplateController : Controller
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<TemplateController> _logger;

    public TemplateController(IApiClientService apiClient, ILogger<TemplateController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var templates = await _apiClient.GetAllFormTemplatesAsync();
        return View(templates);
    }

    public async Task<IActionResult> Create()
    {
        var model = new FormTemplateFormViewModel
        {
            Institutions = await _apiClient.GetInstitutionsAsync(),
            IsActive = true
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FormTemplateFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Remove empty fields (those that were added by JS but left completely blank)
            model.TemplateFields.RemoveAll(f => string.IsNullOrWhiteSpace(f.FieldName));

            // Set OrderNo based on their position in the list and ensure Id is 0 for new fields
            for (int i = 0; i < model.TemplateFields.Count; i++)
            {
                model.TemplateFields[i].OrderNo = i + 1;
                model.TemplateFields[i].Id ??= 0;
            }

            var success = await _apiClient.CreateFormTemplateAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "Şablon başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            
            ModelState.AddModelError("", "Şablon kaydedilirken bir hata oluştu.");
        }

        // If we got this far, something failed, redisplay form
        model.Institutions = await _apiClient.GetInstitutionsAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var template = await _apiClient.GetFormTemplateByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        var model = new FormTemplateFormViewModel
        {
            Id = template.Id,
            InstitutionId = template.InstitutionId,
            Name = template.Name,
            Description = template.Description,
            IsActive = template.IsActive,
            Institutions = await _apiClient.GetInstitutionsAsync(),
            TemplateFields = template.TemplateFields.Select(f => new TemplateFieldFormViewModel
            {
                Id = f.Id,
                FieldName = f.FieldName,
                Regex = f.Regex,
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

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FormTemplateFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            // Remove empty fields
            model.TemplateFields.RemoveAll(f => string.IsNullOrWhiteSpace(f.FieldName));

            // Reassign OrderNo and ensure Id is 0 for new fields
            for (int i = 0; i < model.TemplateFields.Count; i++)
            {
                model.TemplateFields[i].OrderNo = i + 1;
                model.TemplateFields[i].Id ??= 0;
            }

            var success = await _apiClient.UpdateFormTemplateAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Şablon başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Şablon güncellenirken bir hata oluştu.");
        }

        model.Institutions = await _apiClient.GetInstitutionsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _apiClient.DeleteFormTemplateAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Şablon başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Şablon silinirken bir hata oluştu.";
        }

        return RedirectToAction(nameof(Index));
    }
}
