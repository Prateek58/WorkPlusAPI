using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs.HRDTOs;
using WorkPlusAPI.WorkPlus.Model.HR;
using Microsoft.EntityFrameworkCore;

namespace WorkPlusAPI.WorkPlus.Service.HR;

public class AttendanceService : IAttendanceService
{
    private readonly HRDbContext _context;
    private readonly WorkPlusContext _workPlusContext;

    public AttendanceService(HRDbContext context, WorkPlusContext workPlusContext)
    {
        _context = context;
        _workPlusContext = workPlusContext;
    }

    public async Task<IEnumerable<AttendanceDTO>> GetAttendanceAsync(DateTime? date = null, int? workerId = null)
    {
        var query = _context.HrAttendances.AsQueryable();
        
        if (date.HasValue)
            query = query.Where(a => a.AttendanceDate == DateOnly.FromDateTime(date.Value));
            
        if (workerId.HasValue)
            query = query.Where(a => a.WorkerId == workerId.Value);

        var attendance = await query
            .OrderByDescending(a => a.AttendanceDate)
            .ThenBy(a => a.WorkerId)
            .ToListAsync();

        // Get worker names from the main WorkPlus database
        var workerIds = attendance.Select(a => a.WorkerId).Distinct().ToList();
        var workers = new Dictionary<int, string>();
        
        if (workerIds.Any())
        {
            var workerData = await _workPlusContext.Workers
                .Where(w => workerIds.Contains(w.WorkerId))
                .Select(w => new { w.WorkerId, w.FullName })
                .ToListAsync();
                
            workers = workerData.ToDictionary(w => w.WorkerId, w => w.FullName);
        }
        
        return attendance.Select(a => new AttendanceDTO
        {
            Id = a.Id,
            WorkerId = a.WorkerId,
            WorkerName = workers.ContainsKey(a.WorkerId) ? workers[a.WorkerId] : $"Worker {a.WorkerId}",
            AttendanceDate = a.AttendanceDate.ToDateTime(TimeOnly.MinValue),
            CheckInTime = a.CheckInTime?.ToString("HH:mm"),
            CheckOutTime = a.CheckOutTime?.ToString("HH:mm"),
            Shift = a.Shift ?? "Full Day",
            Status = a.Status ?? "Present",
            HalfDayType = a.HalfDayType,
            LeaveTypeId = a.LeaveTypeId,
            LeaveTypeName = "", // Will be populated from leave type join if needed
            Remarks = a.Remarks,
            MarkedBy = a.MarkedBy,
            MarkedByName = "", // Will be populated from user join if needed
            MarkedAt = a.MarkedAt ?? DateTime.Now
        });
    }

    public async Task<AttendanceDTO?> GetAttendanceByIdAsync(int id)
    {
        var attendance = await _context.HrAttendances.FindAsync(id);
        if (attendance == null) return null;

        // Get worker name from the main WorkPlus database
        var worker = await _workPlusContext.Workers
            .Where(w => w.WorkerId == attendance.WorkerId)
            .Select(w => w.FullName)
            .FirstOrDefaultAsync();

        return new AttendanceDTO
        {
            Id = attendance.Id,
            WorkerId = attendance.WorkerId,
            WorkerName = worker ?? $"Worker {attendance.WorkerId}",
            AttendanceDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue),
            CheckInTime = attendance.CheckInTime?.ToString("HH:mm"),
            CheckOutTime = attendance.CheckOutTime?.ToString("HH:mm"),
            Shift = attendance.Shift ?? "Full Day",
            Status = attendance.Status ?? "Present",
            HalfDayType = attendance.HalfDayType,
            LeaveTypeId = attendance.LeaveTypeId,
            LeaveTypeName = "",
            Remarks = attendance.Remarks,
            MarkedBy = attendance.MarkedBy,
            MarkedByName = "",
            MarkedAt = attendance.MarkedAt ?? DateTime.Now
        };
    }

    public async Task<AttendanceDTO> MarkAttendanceAsync(CreateAttendanceDTO createDto, int markedByUserId)
    {
        var attendanceDateOnly = DateOnly.FromDateTime(createDto.AttendanceDate);
        
        // Check if worker has approved or pending leave for this date
        var leaveRequest = await _context.HrLeaveRequests
            .FirstOrDefaultAsync(lr => lr.WorkerId == createDto.WorkerId &&
                          (lr.Status == "Approved" || lr.Status == "Pending") &&
                          lr.FromDate <= attendanceDateOnly &&
                          lr.ToDate >= attendanceDateOnly);

        if (leaveRequest != null)
        {
            var statusText = leaveRequest.Status == "Approved" ? "approved" : "pending";
            var actionText = leaveRequest.Status == "Approved" 
                ? "The leave request must be cancelled or the attendance marked as 'On Leave'"
                : "Please approve or reject the leave request first";
            throw new InvalidOperationException($"Cannot mark attendance. Worker has {statusText} leave request for {createDto.AttendanceDate:dd/MM/yyyy}. {actionText}.");
        }

        // Check if attendance already exists for this worker and date
        var existingAttendance = await _context.HrAttendances
            .FirstOrDefaultAsync(a => a.WorkerId == createDto.WorkerId && 
                                    a.AttendanceDate == attendanceDateOnly);

        if (existingAttendance != null)
        {
            throw new InvalidOperationException("Attendance already marked for this worker on this date. Use update instead.");
        }

        // Validate HalfDayType
        ValidateHalfDayType(createDto.Status, createDto.HalfDayType);

        // Parse check in/out times
        TimeOnly? checkInTime = null;
        TimeOnly? checkOutTime = null;
        
        if (!string.IsNullOrEmpty(createDto.CheckInTime) && TimeOnly.TryParse(createDto.CheckInTime, out var parsedCheckIn))
        {
            checkInTime = parsedCheckIn;
        }
        
        if (!string.IsNullOrEmpty(createDto.CheckOutTime) && TimeOnly.TryParse(createDto.CheckOutTime, out var parsedCheckOut))
        {
            checkOutTime = parsedCheckOut;
        }

        var attendance = new HrAttendance
        {
            WorkerId = createDto.WorkerId,
            AttendanceDate = attendanceDateOnly,
            CheckInTime = checkInTime,
            CheckOutTime = checkOutTime,
            Shift = "Full Day", // Default shift
            Status = createDto.Status,
            HalfDayType = createDto.Status == "Half Day" ? createDto.HalfDayType : null,
            LeaveTypeId = null, // Not used in current implementation
            Remarks = createDto.Remarks,
            MarkedBy = markedByUserId, // Use authenticated user ID
            MarkedAt = DateTime.Now
        };

        _context.HrAttendances.Add(attendance);
        await _context.SaveChangesAsync();
        

        // Get worker name from the main WorkPlus database
        var worker = await _workPlusContext.Workers
            .Where(w => w.WorkerId == attendance.WorkerId)
            .Select(w => w.FullName)
            .FirstOrDefaultAsync();

        return new AttendanceDTO
        {
            Id = attendance.Id,
            WorkerId = attendance.WorkerId,
            WorkerName = worker ?? $"Worker {attendance.WorkerId}",
            AttendanceDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue),
            CheckInTime = attendance.CheckInTime?.ToString("HH:mm"),
            CheckOutTime = attendance.CheckOutTime?.ToString("HH:mm"),
            Shift = attendance.Shift ?? "Full Day",
            Status = attendance.Status ?? "Present",
            HalfDayType = attendance.HalfDayType,
            LeaveTypeId = attendance.LeaveTypeId,
            Remarks = attendance.Remarks,
            MarkedBy = attendance.MarkedBy,
            MarkedAt = attendance.MarkedAt ?? DateTime.Now
        };
    }

    public async Task<BulkAttendanceResultDTO> BulkMarkAttendanceAsync(BulkAttendanceDTO bulkDto, int markedByUserId)
    {
        // Validate HalfDayType
        ValidateHalfDayType(bulkDto.Status, bulkDto.HalfDayType);

        var attendanceList = new List<HrAttendance>();
        var updatedAttendanceList = new List<HrAttendance>();
        var resultList = new List<AttendanceDTO>();
        var skippedWorkers = new List<string>();
        var attendanceDateOnly = DateOnly.FromDateTime(bulkDto.AttendanceDate);

        // Get workers with approved or pending leave for this date
        var workersWithLeave = await _context.HrLeaveRequests
            .Where(lr => bulkDto.WorkerIds.Contains(lr.WorkerId) &&
                        (lr.Status == "Approved" || lr.Status == "Pending") &&
                        lr.FromDate <= attendanceDateOnly &&
                        lr.ToDate >= attendanceDateOnly)
            .Select(lr => new { lr.WorkerId, lr.Status })
            .ToListAsync();

        // Get worker names for messaging
        var workerNames = new Dictionary<int, string>();
        var leaveStatuses = new Dictionary<int, string>();
        if (workersWithLeave.Any())
        {
            var workerIds = workersWithLeave.Select(w => w.WorkerId).ToList();
            var workerData = await _workPlusContext.Workers
                .Where(w => workerIds.Contains(w.WorkerId))
                .Select(w => new { w.WorkerId, w.FullName })
                .ToListAsync();
                
            workerNames = workerData.ToDictionary(w => w.WorkerId, w => w.FullName);
            leaveStatuses = workersWithLeave.ToDictionary(w => w.WorkerId, w => w.Status);
        }

        foreach (var workerId in bulkDto.WorkerIds)
        {
            // Skip workers with approved or pending leave
            if (workersWithLeave.Any(w => w.WorkerId == workerId))
            {
                var workerName = workerNames.ContainsKey(workerId) ? workerNames[workerId] : $"Worker {workerId}";
                var leaveStatus = leaveStatuses.ContainsKey(workerId) ? leaveStatuses[workerId].ToLower() : "pending";
                skippedWorkers.Add($"{workerName} ({leaveStatus} leave)");
                continue;
            }

            // Check if attendance already exists
            var existingAttendance = await _context.HrAttendances
                .FirstOrDefaultAsync(a => a.WorkerId == workerId && 
                                        a.AttendanceDate == attendanceDateOnly);

            if (existingAttendance != null)
            {
                // Update existing attendance
                existingAttendance.Status = bulkDto.Status;
                existingAttendance.HalfDayType = bulkDto.Status == "Half Day" ? bulkDto.HalfDayType : null;
                existingAttendance.Remarks = bulkDto.Remarks;
                existingAttendance.MarkedBy = markedByUserId;
                existingAttendance.MarkedAt = DateTime.Now;
                
                updatedAttendanceList.Add(existingAttendance);
            }
            else
            {
                // Create new attendance
                var attendance = new HrAttendance
                {
                    WorkerId = workerId,
                    AttendanceDate = attendanceDateOnly,
                    Shift = "Full Day", // Default shift
                    Status = bulkDto.Status,
                    HalfDayType = bulkDto.Status == "Half Day" ? bulkDto.HalfDayType : null,
                    Remarks = bulkDto.Remarks,
                    MarkedBy = markedByUserId, // Use authenticated user ID
                    MarkedAt = DateTime.Now
                };

                attendanceList.Add(attendance);
            }
        }

        // Add new attendance records
        if (attendanceList.Any())
        {
            _context.HrAttendances.AddRange(attendanceList);
        }

        // Save all changes (new and updated)
        await _context.SaveChangesAsync();

        // Combine all processed attendance records
        var allProcessedAttendance = attendanceList.Concat(updatedAttendanceList).ToList();

        if (allProcessedAttendance.Any())
        {
            // Get worker names from the main WorkPlus database
            var allWorkerIds = allProcessedAttendance.Select(a => a.WorkerId).Distinct().ToList();
            var workers = new Dictionary<int, string>();
            
            if (allWorkerIds.Any())
            {
                var workerData = await _workPlusContext.Workers
                    .Where(w => allWorkerIds.Contains(w.WorkerId))
                    .Select(w => new { w.WorkerId, w.FullName })
                    .ToListAsync();
                    
                workers = workerData.ToDictionary(w => w.WorkerId, w => w.FullName);
            }

            resultList = allProcessedAttendance.Select(a => new AttendanceDTO
            {
                Id = a.Id,
                WorkerId = a.WorkerId,
                WorkerName = workers.ContainsKey(a.WorkerId) ? workers[a.WorkerId] : $"Worker {a.WorkerId}",
                AttendanceDate = a.AttendanceDate.ToDateTime(TimeOnly.MinValue),
                CheckInTime = a.CheckInTime?.ToString("HH:mm"),
                CheckOutTime = a.CheckOutTime?.ToString("HH:mm"),
                Shift = a.Shift ?? "Full Day",
                Status = a.Status ?? "Present",
                HalfDayType = a.HalfDayType,
                Remarks = a.Remarks,
                MarkedBy = a.MarkedBy,
                MarkedAt = a.MarkedAt ?? DateTime.Now
            }).ToList();
        }

        // Create result message
        var message = $"Attendance processed for {resultList.Count} workers.";
        if (skippedWorkers.Any())
        {
            message += $" Skipped {skippedWorkers.Count} workers with approved or pending leave.";
        }

        return new BulkAttendanceResultDTO
        {
            ProcessedAttendance = resultList,
            SkippedWorkers = skippedWorkers,
            Message = message
        };
    }

    public async Task<bool> UpdateAttendanceAsync(int id, CreateAttendanceDTO updateDto, int markedByUserId)
    {
        var attendance = await _context.HrAttendances.FindAsync(id);
        if (attendance == null) return false;

        var attendanceDateOnly = DateOnly.FromDateTime(updateDto.AttendanceDate);
        
        // Check if worker has approved or pending leave for this date (only if changing the date)
        if (attendance.AttendanceDate != attendanceDateOnly)
        {
            var leaveRequest = await _context.HrLeaveRequests
                .FirstOrDefaultAsync(lr => lr.WorkerId == updateDto.WorkerId &&
                              (lr.Status == "Approved" || lr.Status == "Pending") &&
                              lr.FromDate <= attendanceDateOnly &&
                              lr.ToDate >= attendanceDateOnly);

            if (leaveRequest != null)
            {
                var statusText = leaveRequest.Status == "Approved" ? "approved" : "pending";
                var actionText = leaveRequest.Status == "Approved" 
                    ? "The leave request must be cancelled or the attendance marked as 'On Leave'"
                    : "Please approve or reject the leave request first";
                throw new InvalidOperationException($"Cannot update attendance. Worker has {statusText} leave request for {updateDto.AttendanceDate:dd/MM/yyyy}. {actionText}.");
            }
        }

        // Validate HalfDayType
        ValidateHalfDayType(updateDto.Status, updateDto.HalfDayType);

        // Parse check in/out times
        TimeOnly? checkInTime = null;
        TimeOnly? checkOutTime = null;
        
        if (!string.IsNullOrEmpty(updateDto.CheckInTime) && TimeOnly.TryParse(updateDto.CheckInTime, out var parsedCheckIn))
        {
            checkInTime = parsedCheckIn;
        }
        
        if (!string.IsNullOrEmpty(updateDto.CheckOutTime) && TimeOnly.TryParse(updateDto.CheckOutTime, out var parsedCheckOut))
        {
            checkOutTime = parsedCheckOut;
        }

        attendance.AttendanceDate = attendanceDateOnly;
        attendance.CheckInTime = checkInTime;
        attendance.CheckOutTime = checkOutTime;
        attendance.Shift = "Full Day"; // Default shift
        attendance.Status = updateDto.Status;
        attendance.HalfDayType = updateDto.Status == "Half Day" ? updateDto.HalfDayType : null;
        attendance.LeaveTypeId = null; // Not used in current implementation
        attendance.Remarks = updateDto.Remarks;
        attendance.MarkedBy = markedByUserId; // Use authenticated user ID
        attendance.MarkedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAttendanceAsync(int id)
    {
        var attendance = await _context.HrAttendances.FindAsync(id);
        if (attendance == null) return false;

        _context.HrAttendances.Remove(attendance);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<AttendanceReportDTO>> GetAttendanceReportAsync(DateTime fromDate, DateTime toDate, int? workerId = null)
    {
        var fromDateOnly = DateOnly.FromDateTime(fromDate);
        var toDateOnly = DateOnly.FromDateTime(toDate);
        
        var query = _context.HrAttendances
            .Where(a => a.AttendanceDate >= fromDateOnly && a.AttendanceDate <= toDateOnly);

        if (workerId.HasValue)
            query = query.Where(a => a.WorkerId == workerId.Value);

        var attendanceData = await query
            .GroupBy(a => a.WorkerId)
            .Select(g => new AttendanceReportDTO
            {
                WorkerId = g.Key,
                WorkerName = "", // Will need to join with worker table
                PresentDays = g.Count(a => a.Status == "Present"),
                AbsentDays = g.Count(a => a.Status == "Absent"),
                HalfDays = g.Count(a => a.Status == "Half Day"),
                LeaveDays = g.Count(a => a.Status == "Leave"),
                CompOffDays = g.Count(a => a.Status == "Comp Off"),
                TotalMarkedDays = g.Count()
            })
            .ToListAsync();

        return attendanceData;
    }

    public async Task<IEnumerable<AttendanceReportDTO>> GetCurrentMonthAttendanceAsync()
    {
        var currentMonth = DateTime.Now;
        var fromDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        return await GetAttendanceReportAsync(fromDate, toDate);
    }

    public async Task<bool> CanMarkAttendanceAsync(int workerId, DateTime date)
    {
        // Check if attendance already exists
        var dateOnly = DateOnly.FromDateTime(date);
        var exists = await _context.HrAttendances
            .AnyAsync(a => a.WorkerId == workerId && a.AttendanceDate == dateOnly);

        return !exists;
    }

    public async Task<bool> IsHolidayAsync(DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        var holiday = await _context.HrMasterHolidays
            .FirstOrDefaultAsync(h => h.HolidayDate == dateOnly && h.IsActive == true);

        return holiday != null;
    }

    public async Task<bool> HasApprovedLeaveAsync(int workerId, DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        var hasApprovedLeave = await _context.HrLeaveRequests
            .AnyAsync(lr => lr.WorkerId == workerId &&
                          lr.Status == "Approved" &&
                          lr.FromDate <= dateOnly &&
                          lr.ToDate >= dateOnly);

        return hasApprovedLeave;
    }

    public async Task<bool> HasLeaveRequestAsync(int workerId, DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        var hasLeaveRequest = await _context.HrLeaveRequests
            .AnyAsync(lr => lr.WorkerId == workerId &&
                          (lr.Status == "Approved" || lr.Status == "Pending") &&
                          lr.FromDate <= dateOnly &&
                          lr.ToDate >= dateOnly);

        return hasLeaveRequest;
    }

    public async Task<(bool hasLeave, string? leaveStatus, string? leaveType)> GetLeaveRequestDetailsAsync(int workerId, DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        var leaveRequest = await _context.HrLeaveRequests
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.WorkerId == workerId &&
                          (lr.Status == "Approved" || lr.Status == "Pending") &&
                          lr.FromDate <= dateOnly &&
                          lr.ToDate >= dateOnly);

        if (leaveRequest == null)
        {
            return (false, null, null);
        }

        return (true, leaveRequest.Status, leaveRequest.LeaveType?.Name);
    }

    private void ValidateHalfDayType(string status, string? halfDayType)
    {
        if (status == "Half Day")
        {
            if (string.IsNullOrEmpty(halfDayType))
            {
                throw new ArgumentException("Half day type is required when status is 'Half Day'");
            }
            
            if (halfDayType != "First Half" && halfDayType != "Second Half")
            {
                throw new ArgumentException("Half day type must be either 'First Half' or 'Second Half'");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(halfDayType))
            {
                throw new ArgumentException("Half day type should only be specified when status is 'Half Day'");
            }
        }
    }
} 