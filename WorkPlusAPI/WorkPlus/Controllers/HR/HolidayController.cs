using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.Model.HR;

namespace WorkPlusAPI.WorkPlus.Controllers.HR;

[ApiController]
[Route("api/hr/[controller]")]
[Authorize]
public class HolidayController : ControllerBase
{
    private readonly HRDbContext _context;
    private readonly ILogger<HolidayController> _logger;

    public HolidayController(HRDbContext context, ILogger<HolidayController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetHolidays()
    {
        try
        {
            var holidays = await _context.HrMasterHolidays
                .Where(h => h.IsActive == true)
                .OrderBy(h => h.HolidayDate)
                .Select(h => new
                {
                    id = h.Id,
                    holidayDate = h.HolidayDate.ToString("yyyy-MM-dd"),
                    name = h.Name,
                    isOptional = h.IsOptional ?? false,
                    isActive = h.IsActive ?? true
                })
                .ToListAsync();

            return Ok(holidays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving holidays");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetHoliday(int id)
    {
        try
        {
            var holiday = await _context.HrMasterHolidays
                .Where(h => h.Id == id)
                .Select(h => new
                {
                    id = h.Id,
                    holidayDate = h.HolidayDate.ToString("yyyy-MM-dd"),
                    name = h.Name,
                    isOptional = h.IsOptional ?? false,
                    isActive = h.IsActive ?? true
                })
                .FirstOrDefaultAsync();

            if (holiday == null)
                return NotFound();

            return Ok(holiday);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving holiday {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateHoliday([FromBody] CreateHolidayDto dto)
    {
        try
        {
            var holiday = new HrMasterHoliday
            {
                HolidayDate = DateOnly.Parse(dto.HolidayDate),
                Name = dto.Name,
                IsOptional = dto.IsOptional,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.HrMasterHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            var result = new
            {
                id = holiday.Id,
                holidayDate = holiday.HolidayDate.ToString("yyyy-MM-dd"),
                name = holiday.Name,
                isOptional = holiday.IsOptional ?? false,
                isActive = holiday.IsActive ?? true
            };

            return CreatedAtAction(nameof(GetHoliday), new { id = holiday.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating holiday");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateHoliday(int id, [FromBody] UpdateHolidayDto dto)
    {
        try
        {
            var holiday = await _context.HrMasterHolidays.FindAsync(id);
            if (holiday == null)
                return NotFound();

            if (!string.IsNullOrEmpty(dto.HolidayDate))
                holiday.HolidayDate = DateOnly.Parse(dto.HolidayDate);
            if (!string.IsNullOrEmpty(dto.Name))
                holiday.Name = dto.Name;
            if (dto.IsOptional.HasValue)
                holiday.IsOptional = dto.IsOptional;
            if (dto.IsActive.HasValue)
                holiday.IsActive = dto.IsActive;

            holiday.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var result = new
            {
                id = holiday.Id,
                holidayDate = holiday.HolidayDate.ToString("yyyy-MM-dd"),
                name = holiday.Name,
                isOptional = holiday.IsOptional ?? false,
                isActive = holiday.IsActive ?? true
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating holiday {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHoliday(int id)
    {
        try
        {
            var holiday = await _context.HrMasterHolidays.FindAsync(id);
            if (holiday == null)
                return NotFound();

            holiday.IsActive = false;
            holiday.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting holiday {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class CreateHolidayDto
{
    public string HolidayDate { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsOptional { get; set; } = false;
}

public class UpdateHolidayDto
{
    public string? HolidayDate { get; set; }
    public string? Name { get; set; }
    public bool? IsOptional { get; set; }
    public bool? IsActive { get; set; }
} 