using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("institutions")]
    public async Task<IActionResult> GetInstitutions()
    {
        var institutions = await _lookupService.GetInstitutionsAsync();
        return Ok(institutions);
    }

    [HttpGet("templates/{institutionId}")]
    public async Task<IActionResult> GetTemplatesByInstitution(int institutionId)
    {
        var templates = await _lookupService.GetTemplatesByInstitutionAsync(institutionId);
        return Ok(templates);
    }
}
