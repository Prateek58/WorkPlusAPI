using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.Model.HR;

namespace WorkPlusAPI.WorkPlus.Controllers.HR;

[ApiController]
[Route("api/hr/calendar-config")]
[Authorize]
public class CalendarConfigController : ControllerBase
{
    private readonly HRDbContext _context;
    private readonly ILogger<CalendarConfigController> _logger;

    public CalendarConfigController(HRDbContext context, ILogger<CalendarConfigController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCalendarConfigs()
    {
        try
        {
            var configs = await _context.HrMasterCalendarConfigs
                .OrderBy(c => c.Id)
                .Select(c => new
                {
                    id = c.Id,
                    dayOfWeek = c.DayOfWeek,
                    isWorkingDay = c.IsWorkingDay ?? true
                })
                .ToListAsync();

            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving calendar configurations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetCalendarConfig(int id)
    {
        try
        {
            var config = await _context.HrMasterCalendarConfigs
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    id = c.Id,
                    dayOfWeek = c.DayOfWeek,
                    isWorkingDay = c.IsWorkingDay ?? true
                })
                .FirstOrDefaultAsync();

            if (config == null)
                return NotFound();

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving calendar config {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut]
    public async Task<ActionResult<IEnumerable<object>>> UpdateCalendarConfigs([FromBody] UpdateCalendarConfigsDto dto)
    {
        try
        {
            foreach (var configDto in dto.Configs)
            {
                var config = await _context.HrMasterCalendarConfigs
                    .FirstOrDefaultAsync(c => c.DayOfWeek == configDto.DayOfWeek);

                if (config != null)
                {
                    config.IsWorkingDay = configDto.IsWorkingDay;
                    config.UpdatedAt = DateTime.Now;
                }
                else
                {
                    config = new HrMasterCalendarConfig
                    {
                        DayOfWeek = configDto.DayOfWeek,
                        IsWorkingDay = configDto.IsWorkingDay,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.HrMasterCalendarConfigs.Add(config);
                }
            }

            await _context.SaveChangesAsync();

            var updatedConfigs = await _context.HrMasterCalendarConfigs
                .OrderBy(c => c.Id)
                .Select(c => new
                {
                    id = c.Id,
                    dayOfWeek = c.DayOfWeek,
                    isWorkingDay = c.IsWorkingDay ?? true
                })
                .ToListAsync();

            return Ok(updatedConfigs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating calendar configurations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateCalendarConfig([FromBody] CreateCalendarConfigDto dto)
    {
        try
        {
            var config = new HrMasterCalendarConfig
            {
                DayOfWeek = dto.DayOfWeek,
                IsWorkingDay = dto.IsWorkingDay,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.HrMasterCalendarConfigs.Add(config);
            await _context.SaveChangesAsync();

            var result = new
            {
                id = config.Id,
                dayOfWeek = config.DayOfWeek,
                isWorkingDay = config.IsWorkingDay ?? true
            };

            return CreatedAtAction(nameof(GetCalendarConfig), new { id = config.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating calendar config");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class CreateCalendarConfigDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsWorkingDay { get; set; } = true;
}

public class UpdateCalendarConfigsDto
{
    public List<CalendarConfigDto> Configs { get; set; } = new();
}

public class CalendarConfigDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsWorkingDay { get; set; } = true;
} 