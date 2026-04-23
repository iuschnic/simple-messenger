using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth.BL.InputPorts;
using Auth.BL.Models;

namespace Auth.API.Controllers;

[ApiController]
[Route("/api/v1/auth/")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("register")]
    public Task<ActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        ArgumentNullException.ThrowIfNull(registerRequest);
        return _authService.RegisterAsync(registerRequest);
    }

    [HttpPost("login")]
    public Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        ArgumentNullException.ThrowIfNull(loginRequest);
        return _authService.LoginAsync(loginRequest);
    }
}
