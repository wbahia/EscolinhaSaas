using EscolinhaSaaS.Application.DTOs;
using EscolinhaSaaS.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EscolinhaSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterEscolinhaAsync(request);
        return Ok(new { success = true, tenantId = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);
        return Ok(new { success = true, token });
    }
}