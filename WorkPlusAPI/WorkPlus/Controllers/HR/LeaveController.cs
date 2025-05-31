using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WorkPlusAPI.WorkPlus.DTOs.HRDTOs;
using WorkPlusAPI.WorkPlus.Service.HR;

namespace WorkPlusAPI.WorkPlus.Controllers.HR;

[ApiController]
[Route("api/hr/[controller]")]
[Authorize] // Require authentication for all HR operations
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
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

    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<LeaveTypeDTO>>> GetLeaveTypes()
    {
        var leaveTypes = await _leaveService.GetLeaveTypesAsync();
        return Ok(leaveTypes);
    }

    [HttpGet("types/{id}")]
    public async Task<ActionResult<LeaveTypeDTO>> GetLeaveType(int id)
    {
        var leaveType = await _leaveService.GetLeaveTypeByIdAsync(id);
        if (leaveType == null) return NotFound();
        return Ok(leaveType);
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDTO>>> GetLeaveRequests(
        [FromQuery] int? workerId = null,
        [FromQuery] string? status = null)
    {
        var requests = await _leaveService.GetLeaveRequestsAsync(workerId, status);
        return Ok(requests);
    }

    [HttpGet("requests/{id}")]
    public async Task<ActionResult<LeaveRequestDTO>> GetLeaveRequest(int id)
    {
        var request = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPost("requests")]
    public async Task<ActionResult<LeaveRequestDTO>> CreateLeaveRequest(CreateLeaveRequestDTO createDto)
    {
        var request = await _leaveService.CreateLeaveRequestAsync(createDto);
        return CreatedAtAction(nameof(GetLeaveRequest), new { id = request.Id }, request);
    }

    [HttpPut("requests/{id}/approve")]
    public async Task<ActionResult<LeaveRequestDTO>> ApproveLeaveRequest(int id, ApproveLeaveRequestDTO approveDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            approveDto.Id = id;
            approveDto.ApprovedBy = currentUserId;
            var request = await _leaveService.ApproveLeaveRequestAsync(approveDto);
            return Ok(request);
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

    [HttpPut("requests/{id}/cancel")]
    public async Task<ActionResult> CancelLeaveRequest(int id, [FromBody] int cancelledBy)
    {
        var result = await _leaveService.CancelLeaveRequestAsync(id, cancelledBy);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("balance/{workerId}")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDTO>>> GetLeaveBalance(
        int workerId,
        [FromQuery] int? year = null)
    {
        var balance = await _leaveService.GetLeaveBalanceAsync(workerId, year);
        return Ok(balance);
    }

    [HttpPost("balance/allocate")]
    public async Task<ActionResult<LeaveBalanceDTO>> AllocateLeaveBalance(AllocateLeaveBalanceDTO allocateDto)
    {
        var balance = await _leaveService.AllocateLeaveBalanceAsync(allocateDto);
        return Ok(balance);
    }

    [HttpPost("balance/auto-allocate/{workerId}/{year}")]
    public async Task<ActionResult> AutoAllocateLeaveBalance(int workerId, int year)
    {
        var result = await _leaveService.AutoAllocateLeaveBalanceAsync(workerId, year);
        if (!result) return BadRequest("Failed to auto-allocate leave balance");
        return Ok();
    }
} 