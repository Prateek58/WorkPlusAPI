using WorkPlusAPI.WorkPlus.DTOs.HRDTOs;

namespace WorkPlusAPI.WorkPlus.Service.HR;

public interface IAttendanceService
{
    // Attendance Management
    Task<IEnumerable<AttendanceDTO>> GetAttendanceAsync(DateTime? date = null, int? workerId = null);
    Task<AttendanceDTO?> GetAttendanceByIdAsync(int id);
    Task<AttendanceDTO> MarkAttendanceAsync(CreateAttendanceDTO createDto, int markedByUserId);
    Task<BulkAttendanceResultDTO> BulkMarkAttendanceAsync(BulkAttendanceDTO bulkDto, int markedByUserId);
    Task<bool> UpdateAttendanceAsync(int id, CreateAttendanceDTO updateDto, int markedByUserId);
    Task<bool> DeleteAttendanceAsync(int id);
    
    // Reports
    Task<IEnumerable<AttendanceReportDTO>> GetAttendanceReportAsync(DateTime fromDate, DateTime toDate, int? workerId = null);
    Task<IEnumerable<AttendanceReportDTO>> GetCurrentMonthAttendanceAsync();
    
    // Validation
    Task<bool> CanMarkAttendanceAsync(int workerId, DateTime date);
    Task<bool> HasLeaveRequestAsync(int workerId, DateTime date);
    Task<(bool hasLeave, string? leaveStatus, string? leaveType)> GetLeaveRequestDetailsAsync(int workerId, DateTime date);
    Task<bool> IsHolidayAsync(DateTime date);
} 