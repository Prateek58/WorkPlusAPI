using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkPlusAPI.Models.Auth;
using WorkPlusAPI.Services;

namespace WorkPlusAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var response = await _authService.Login(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var response = await _authService.Register(request);
        if (response == null)
        {
            return BadRequest(new { message = "Username or email already exists" });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("test")]
    public ActionResult<object> TestAuth()
    {
        return Ok(new { 
            message = "You are authenticated!", 
            username = User.Identity?.Name,
            roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-test")]
    public ActionResult<object> TestAdminAuth()
    {
        return Ok(new { message = "You are authenticated as an Admin!" });
    }
} 