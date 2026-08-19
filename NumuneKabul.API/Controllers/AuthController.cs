using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı girişi — JWT token döner.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Kullanıcı adı ve şifre zorunludur.");

        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        return Ok(result);
    }
}
