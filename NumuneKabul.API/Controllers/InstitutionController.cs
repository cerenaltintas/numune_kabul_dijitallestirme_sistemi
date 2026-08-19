using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InstitutionController : ControllerBase
{
    private readonly IInstitutionService _institutionService;

    public InstitutionController(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var dtos = await _institutionService.GetAllInstitutionsAsync();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var institution = await _institutionService.GetInstitutionByIdAsync(id);
        if (institution == null) return NotFound();

        return Ok(institution);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInstitutionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var institution = await _institutionService.CreateInstitutionAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = institution.Id }, institution);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInstitutionDto dto)
    {
        if (id != dto.Id) return BadRequest("ID uyumsuzluğu.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _institutionService.UpdateInstitutionAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _institutionService.DeleteInstitutionAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
