using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormTemplateController : ControllerBase
{
    private readonly IFormTemplateService _formTemplateService;

    public FormTemplateController(IFormTemplateService formTemplateService)
    {
        _formTemplateService = formTemplateService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FormTemplateDto>>> GetAll()
    {
        var templates = await _formTemplateService.GetAllAsync();
        return Ok(templates);
    }

    [HttpGet("institution/{institutionId}")]
    public async Task<ActionResult<List<FormTemplateDto>>> GetByInstitutionId(int institutionId)
    {
        var templates = await _formTemplateService.GetByInstitutionIdAsync(institutionId);
        return Ok(templates);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FormTemplateDto>> GetById(int id)
    {
        var template = await _formTemplateService.GetByIdAsync(id);
        if (template == null)
            return NotFound($"Id={id} olan şablon bulunamadı.");

        return Ok(template);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Create(CreateFormTemplateDto dto)
    {
        var id = await _formTemplateService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateFormTemplateDto dto)
    {
        if (id != dto.Id)
            return BadRequest("URL'deki ID ile gövdedeki ID eşleşmiyor.");

        try
        {
            await _formTemplateService.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        //Var olmayan şablonu silmeye çalışırken 204 yerine 404 dön
        var existing = await _formTemplateService.GetByIdAsync(id);
        if (existing == null)
            return NotFound($"Id={id} olan şablon bulunamadı.");

        await _formTemplateService.DeleteAsync(id);
        return NoContent();
    }
}
