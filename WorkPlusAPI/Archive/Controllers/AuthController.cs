using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkPlusAPI.Archive.Services;
using WorkPlusAPI.Archive.Models.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace WorkPlusAPI.Archive.Controllers;

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
        try
        {
            var response = await _authService.Login(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Log the full exception details
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
            logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            
            // Return detailed error information for debugging (REMOVE IN PRODUCTION!)
            return StatusCode(500, new { 
                message = "DETAILED ERROR (DEBUG MODE)",
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace?.Split('\n').Take(10).ToArray(), // First 10 lines only
                errorType = ex.GetType().Name,
                username = request.Username,
                timestamp = DateTime.UtcNow
            });
        }
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

    [HttpPost("updatePassword")]
    public async Task<ActionResult> UpdatePassword(UpdatePasswordRequest request)
    {
        var success = await _authService.UpdatePassword(request);
        if (!success)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new { message = "Password updated successfully" });
    }

    [Authorize]
    [HttpGet("test")]
    public ActionResult<object> TestAuth()
    {
        return Ok(new
        {
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

    [HttpGet("cors-test")]
    public ActionResult<object> TestCors()
    {
        return Ok(new { 
            message = "CORS is working!", 
            timestamp = DateTime.UtcNow,
            origin = Request.Headers["Origin"].FirstOrDefault() ?? "No origin header"
        });
    }

    [HttpPost("cors-test")]
    public ActionResult<object> TestCorsPost([FromBody] object data)
    {
        return Ok(new { 
            message = "CORS POST is working!", 
            timestamp = DateTime.UtcNow,
            receivedData = data
        });
    }

    [HttpGet("health-check")]
    public ActionResult<object> HealthCheck()
    {
        try
        {
            var result = new
            {
                status = "OK",
                timestamp = DateTime.UtcNow,
                server = Environment.MachineName,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                connectionString = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("WorkPlusConnection")?.Replace("Password=", "Password=***"),
                jwtConfigured = !string.IsNullOrEmpty(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:Key"]),
                corsOrigin = Request.Headers["Origin"].FirstOrDefault() ?? "No origin header",
                userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "No user agent"
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "ERROR",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}