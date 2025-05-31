using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WorkPlusAPI.WorkPlus.DTOs.HRDTOs;
using WorkPlusAPI.WorkPlus.Service.HR;

namespace WorkPlusAPI.WorkPlus.Controllers.HR;

[ApiController]
[Route("api/hr/[controller]")]
[Authorize] // Require authentication for all HR operations
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User not authenticated or invalid user ID");
        }
        return userId;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceDTO>>> GetAttendance(
        [FromQuery] DateTime? date = null, 
        [FromQuery] int? workerId = null)
    {
        var attendance = await _attendanceService.GetAttendanceAsync(date, workerId);
        return Ok(attendance);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AttendanceDTO>> GetAttendance(int id)
    {
        var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
        if (attendance == null) return NotFound();
        return Ok(attendance);
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceDTO>> MarkAttendance(CreateAttendanceDTO createDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var attendance = await _attendanceService.MarkAttendanceAsync(createDto, currentUserId);
            return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, attendance);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkAttendanceResultDTO>> BulkMarkAttendance(BulkAttendanceDTO bulkDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _attendanceService.BulkMarkAttendanceAsync(bulkDto, currentUserId);
            
            // Return appropriate status based on whether there were warnings
            if (result.HasWarnings)
            {
                return Ok(new { 
                    success = true, 
                    data = result,
                    warnings = result.SkippedWorkers 
                });
            }
            
            return Ok(new { 
                success = true, 
                data = result 
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAttendance(int id, CreateAttendanceDTO updateDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _attendanceService.UpdateAttendanceAsync(id, updateDto, currentUserId);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAttendance(int id)
    {
        var result = await _attendanceService.DeleteAttendanceAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("report")]
    public async Task<ActionResult<IEnumerable<AttendanceReportDTO>>> GetAttendanceReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int? workerId = null)
    {
        var report = await _attendanceService.GetAttendanceReportAsync(fromDate, toDate, workerId);
        return Ok(report);
    }

    [HttpGet("current-month")]
    public async Task<ActionResult<IEnumerable<AttendanceReportDTO>>> GetCurrentMonthAttendance()
    {
        var report = await _attendanceService.GetCurrentMonthAttendanceAsync();
        return Ok(report);
    }

    [HttpGet("check-leave-conflict")]
    public async Task<ActionResult> CheckLeaveConflict([FromQuery] int workerId, [FromQuery] DateTime date)
    {
        try
        {
            var (hasLeave, leaveStatus, leaveType) = await _attendanceService.GetLeaveRequestDetailsAsync(workerId, date);
            
            if (hasLeave)
            {
                return Ok(new { 
                    hasConflict = true, 
                    leaveStatus = leaveStatus?.ToLower(),
                    leaveType = leaveType,
                    message = $"Worker has {leaveStatus?.ToLower()} {leaveType} leave for {date:dd/MM/yyyy}"
                });
            }
            
            return Ok(new { hasConflict = false });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
} 