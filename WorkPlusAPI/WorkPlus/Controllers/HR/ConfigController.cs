using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.Model.HR;

namespace WorkPlusAPI.WorkPlus.Controllers.HR;

[ApiController]
[Route("api/hr/config")]
[Authorize]
public class ConfigController : ControllerBase
{
    private readonly HRDbContext _context;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(HRDbContext context, ILogger<ConfigController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetConfigs()
    {
        try
        {
            var configs = await _context.HrMasterConfigs
                .Select(c => new
                {
                    configKey = c.ConfigKey,
                    configValue = c.ConfigValue,
                    description = c.Description
                })
                .ToListAsync();

            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HR configurations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{configKey}")]
    public async Task<ActionResult<object>> GetConfig(string configKey)
    {
        try
        {
            var config = await _context.HrMasterConfigs
                .Where(c => c.ConfigKey == configKey)
                .Select(c => new
                {
                    configKey = c.ConfigKey,
                    configValue = c.ConfigValue,
                    description = c.Description
                })
                .FirstOrDefaultAsync();

            if (config == null)
                return NotFound();

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HR config {ConfigKey}", configKey);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{configKey}")]
    public async Task<ActionResult<object>> UpdateConfig(string configKey, [FromBody] UpdateConfigDto dto)
    {
        try
        {
            var config = await _context.HrMasterConfigs.FindAsync(configKey);
            
            if (config == null)
            {
                // Create new config if it doesn't exist
                config = new HrMasterConfig
                {
                    ConfigKey = configKey,
                    ConfigValue = dto.ConfigValue,
                    Description = dto.Description,
                    UpdatedAt = DateTime.Now
                };
                _context.HrMasterConfigs.Add(config);
            }
            else
            {
                // Update existing config
                config.ConfigValue = dto.ConfigValue;
                if (!string.IsNullOrEmpty(dto.Description))
                    config.Description = dto.Description;
                config.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            var result = new
            {
                configKey = config.ConfigKey,
                configValue = config.ConfigValue,
                description = config.Description
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating HR config {ConfigKey}", configKey);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateConfig([FromBody] CreateConfigDto dto)
    {
        try
        {
            var config = new HrMasterConfig
            {
                ConfigKey = dto.ConfigKey,
                ConfigValue = dto.ConfigValue,
                Description = dto.Description,
                UpdatedAt = DateTime.Now
            };

            _context.HrMasterConfigs.Add(config);
            await _context.SaveChangesAsync();

            var result = new
            {
                configKey = config.ConfigKey,
                configValue = config.ConfigValue,
                description = config.Description
            };

            return CreatedAtAction(nameof(GetConfig), new { configKey = config.ConfigKey }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating HR config");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{configKey}")]
    public async Task<ActionResult> DeleteConfig(string configKey)
    {
        try
        {
            var config = await _context.HrMasterConfigs.FindAsync(configKey);
            if (config == null)
                return NotFound();

            _context.HrMasterConfigs.Remove(config);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting HR config {ConfigKey}", configKey);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class CreateConfigDto
{
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateConfigDto
{
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
} 