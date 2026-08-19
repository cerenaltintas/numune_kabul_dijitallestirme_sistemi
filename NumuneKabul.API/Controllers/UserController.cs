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
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var dtos = await _userService.GetAllUsersAsync();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (id != dto.Id) return BadRequest("ID uyumsuzluğu.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _userService.UpdateUserAsync(id, dto);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
