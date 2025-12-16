using Barber.Application.DTOs.Auth;
using Barber.Application.Services.Auth;
using Barber.Infrastructure.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Barber.Api.Controllers;


[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
    
    [HttpPost("google/login")]
    public async Task<IActionResult> GoogleLogin(
        GoogleLoginRequest request, 
        [FromServices] IOptions<GoogleSettings> googleOptions)
    {
        var googleClientId = googleOptions.Value.ClientId;

        var response = await _authService.LoginWithGoogleAsync(request, googleClientId);

        return Ok(response);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        return Ok(response);
    }
}