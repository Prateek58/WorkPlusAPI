using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Service;

namespace WorkPlusAPI.WorkPlus.Controllers;

[Route("api/user/[controller]")]
[ApiController]
[Authorize]
public class UserSettingsController : ControllerBase
{
    private readonly IUserSettingsService _userSettingsService;
    private readonly ILogger<UserSettingsController> _logger;

    public UserSettingsController(IUserSettingsService userSettingsService, ILogger<UserSettingsController> logger)
    {
        _userSettingsService = userSettingsService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }

    // GET: api/user/usersettings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSettingDTO>>> GetUserSettings()
    {
        try
        {
            var userId = GetCurrentUserId();
            var settings = await _userSettingsService.GetUserSettingsAsync(userId);
            return Ok(settings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user settings");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/user/usersettings/{settingKey}
    [HttpGet("{settingKey}")]
    public async Task<ActionResult<UserSettingDTO>> GetUserSetting(string settingKey)
    {
        try
        {
            var userId = GetCurrentUserId();
            var setting = await _userSettingsService.GetUserSettingAsync(userId, settingKey);
            
            if (setting == null)
                return NotFound($"Setting '{settingKey}' not found");

            return Ok(setting);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user setting {SettingKey}", settingKey);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/user/usersettings
    [HttpPost]
    public async Task<ActionResult<UserSettingDTO>> CreateOrUpdateSetting([FromBody] CreateUserSettingDTO createDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var setting = await _userSettingsService.CreateOrUpdateSettingAsync(userId, createDto);
            return Ok(setting);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating user setting {SettingKey}", createDto.SettingKey);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/user/usersettings/bulk
    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<UserSettingDTO>>> UpdateMultipleSettings([FromBody] BulkUserSettingsDTO bulkDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var settings = await _userSettingsService.UpdateMultipleSettingsAsync(userId, bulkDto.Settings);
            return Ok(settings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating multiple user settings");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/user/usersettings/{settingKey}
    [HttpDelete("{settingKey}")]
    public async Task<ActionResult> DeleteSetting(string settingKey)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _userSettingsService.DeleteSettingAsync(userId, settingKey);
            
            if (!deleted)
                return NotFound($"Setting '{settingKey}' not found");

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user setting {SettingKey}", settingKey);
            return StatusCode(500, "Internal server error");
        }
    }

    // Theme-specific endpoints
    // GET: api/user/usersettings/theme/mode
    [HttpGet("theme/mode")]
    public async Task<ActionResult<string>> GetThemeMode()
    {
        try
        {
            var userId = GetCurrentUserId();
            var themeMode = await _userSettingsService.GetThemeModeAsync(userId);
            return Ok(themeMode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme mode");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/user/usersettings/theme/mode
    [HttpPost("theme/mode")]
    public async Task<ActionResult<UserSettingDTO>> SetThemeMode([FromBody] SetThemeModeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var setting = await _userSettingsService.SetThemeModeAsync(userId, request.Mode);
            return Ok(setting);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting theme mode");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/user/usersettings/theme/colors
    [HttpGet("theme/colors")]
    public async Task<ActionResult<object>> GetThemeColors()
    {
        try
        {
            var userId = GetCurrentUserId();
            var colors = await _userSettingsService.GetThemeColorsAsync(userId);
            return Ok(colors ?? new object()); // Return empty object if no colors set
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme colors");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/user/usersettings/theme/colors
    [HttpPost("theme/colors")]
    public async Task<ActionResult<UserSettingDTO>> SetThemeColors([FromBody] object colors)
    {
        try
        {
            var userId = GetCurrentUserId();
            var setting = await _userSettingsService.SetThemeColorsAsync(userId, colors);
            return Ok(setting);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting theme colors");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class SetThemeModeRequest
{
    public string Mode { get; set; } = string.Empty;
} 