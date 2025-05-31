using WorkPlusAPI.WorkPlus.DTOs.HRDTOs;

namespace WorkPlusAPI.WorkPlus.Service.HR;

public interface ILeaveService
{
    // Leave Types
    Task<IEnumerable<LeaveTypeDTO>> GetLeaveTypesAsync();
    Task<LeaveTypeDTO?> GetLeaveTypeByIdAsync(int id);
    
    // Leave Requests
    Task<IEnumerable<LeaveRequestDTO>> GetLeaveRequestsAsync(int? workerId = null, string? status = null);
    Task<LeaveRequestDTO?> GetLeaveRequestByIdAsync(int id);
    Task<LeaveRequestDTO> CreateLeaveRequestAsync(CreateLeaveRequestDTO createDto);
    Task<LeaveRequestDTO> ApproveLeaveRequestAsync(ApproveLeaveRequestDTO approveDto);
    Task<bool> CancelLeaveRequestAsync(int id, int cancelledBy);
    
    // Leave Balance
    Task<IEnumerable<LeaveBalanceDTO>> GetLeaveBalanceAsync(int workerId, int? year = null);
    Task<LeaveBalanceDTO> AllocateLeaveBalanceAsync(AllocateLeaveBalanceDTO allocateDto);
    Task<bool> AutoAllocateLeaveBalanceAsync(int workerId, int year);
    
    // Validation
    Task<bool> HasSufficientBalanceAsync(int workerId, int leaveTypeId, decimal days, int year);
    Task<bool> HasOverlappingLeaveAsync(int workerId, DateTime fromDate, DateTime toDate);
    Task<decimal> CalculateLeaveDaysAsync(DateTime fromDate, DateTime toDate);
} 