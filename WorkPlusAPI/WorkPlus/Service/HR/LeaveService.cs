using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs.HRDTOs;
using WorkPlusAPI.WorkPlus.Model.HR;
using Microsoft.EntityFrameworkCore;

namespace WorkPlusAPI.WorkPlus.Service.HR;

public class LeaveService : ILeaveService
{
    private readonly HRDbContext _context;
    private readonly WorkPlusContext _workPlusContext;

    public LeaveService(HRDbContext context, WorkPlusContext workPlusContext)
    {
        _context = context;
        _workPlusContext = workPlusContext;
    }

    // Leave Types
    public async Task<IEnumerable<LeaveTypeDTO>> GetLeaveTypesAsync()
    {
        var leaveTypes = await _context.HrMasterLeaveTypes
            .Where(lt => lt.IsActive == true)
            .OrderBy(lt => lt.Name)
            .ToListAsync();

        return leaveTypes.Select(lt => new LeaveTypeDTO
        {
            Id = lt.Id,
            Code = lt.Code ?? "",
            Name = lt.Name ?? "",
            IsPaid = lt.IsPaid ?? false,
            AppliesTo = lt.AppliesTo ?? "All",
            MaxDaysPerYear = lt.MaxDaysPerYear,
            IsActive = lt.IsActive ?? true
        });
    }

    public async Task<LeaveTypeDTO?> GetLeaveTypeByIdAsync(int id)
    {
        var leaveType = await _context.HrMasterLeaveTypes.FindAsync(id);
        if (leaveType == null) return null;

        return new LeaveTypeDTO
        {
            Id = leaveType.Id,
            Code = leaveType.Code ?? "",
            Name = leaveType.Name ?? "",
            IsPaid = leaveType.IsPaid ?? false,
            AppliesTo = leaveType.AppliesTo ?? "All",
            MaxDaysPerYear = leaveType.MaxDaysPerYear,
            IsActive = leaveType.IsActive ?? true
        };
    }

    // Leave Requests
    public async Task<IEnumerable<LeaveRequestDTO>> GetLeaveRequestsAsync(int? workerId = null, string? status = null)
    {
        var query = _context.HrLeaveRequests
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        if (workerId.HasValue)
            query = query.Where(lr => lr.WorkerId == workerId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(lr => lr.Status == status);

        var leaveRequests = await query
            .OrderByDescending(lr => lr.RequestedAt)
            .ToListAsync();

        // Get worker names from the main WorkPlus database
        var workerIds = leaveRequests.Select(lr => lr.WorkerId).Distinct().ToList();
        var workers = new Dictionary<int, string>();
        
        if (workerIds.Any())
        {
            var workerData = await _workPlusContext.Workers
                .Where(w => workerIds.Contains(w.WorkerId))
                .Select(w => new { w.WorkerId, w.FullName })
                .ToListAsync();
                
            workers = workerData.ToDictionary(w => w.WorkerId, w => w.FullName);
        }
        
        return leaveRequests.Select(lr => new LeaveRequestDTO
        {
            Id = lr.Id,
            WorkerId = lr.WorkerId,
            WorkerName = workers.ContainsKey(lr.WorkerId) ? workers[lr.WorkerId] : $"Worker {lr.WorkerId}",
            LeaveTypeId = lr.LeaveTypeId,
            LeaveTypeName = lr.LeaveType?.Name ?? "",
            FromDate = lr.FromDate.ToDateTime(TimeOnly.MinValue),
            ToDate = lr.ToDate.ToDateTime(TimeOnly.MinValue),
            TotalDays = lr.TotalDays,
            Status = lr.Status ?? "Pending",
            Reason = lr.Reason,
            RejectionReason = lr.RejectionReason,
            RequestedAt = lr.RequestedAt ?? DateTime.Now,
            ApprovedBy = lr.ApprovedBy,
            ApprovedByName = "", // Will be populated from user join if needed
            ApprovedAt = lr.ApprovedAt
        });
    }

    public async Task<LeaveRequestDTO?> GetLeaveRequestByIdAsync(int id)
    {
        var leaveRequest = await _context.HrLeaveRequests.FindAsync(id);
        if (leaveRequest == null) return null;

        return new LeaveRequestDTO
        {
            Id = leaveRequest.Id,
            WorkerId = leaveRequest.WorkerId,
            WorkerName = "",
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = "",
            FromDate = leaveRequest.FromDate.ToDateTime(TimeOnly.MinValue),
            ToDate = leaveRequest.ToDate.ToDateTime(TimeOnly.MinValue),
            TotalDays = leaveRequest.TotalDays,
            Status = leaveRequest.Status ?? "Pending",
            Reason = leaveRequest.Reason,
            RejectionReason = leaveRequest.RejectionReason,
            RequestedAt = leaveRequest.RequestedAt ?? DateTime.Now,
            ApprovedBy = leaveRequest.ApprovedBy,
            ApprovedAt = leaveRequest.ApprovedAt
        };
    }

    public async Task<LeaveRequestDTO> CreateLeaveRequestAsync(CreateLeaveRequestDTO createDto)
    {
        var fromDateOnly = DateOnly.FromDateTime(createDto.FromDate);
        var toDateOnly = DateOnly.FromDateTime(createDto.ToDate);
        
        // Calculate total days
        var totalDays = await CalculateLeaveDaysAsync(createDto.FromDate, createDto.ToDate);

        // Check for overlapping leave
        var hasOverlapping = await HasOverlappingLeaveAsync(createDto.WorkerId, createDto.FromDate, createDto.ToDate);
        if (hasOverlapping)
        {
            throw new InvalidOperationException("Leave request overlaps with existing leave");
        }

        // Check sufficient balance
        var currentYear = createDto.FromDate.Year;
        var hasSufficientBalance = await HasSufficientBalanceAsync(createDto.WorkerId, createDto.LeaveTypeId, totalDays, currentYear);
        if (!hasSufficientBalance)
        {
            throw new InvalidOperationException("Insufficient leave balance");
        }

        var leaveRequest = new HrLeaveRequest
        {
            WorkerId = createDto.WorkerId,
            LeaveTypeId = createDto.LeaveTypeId,
            FromDate = fromDateOnly,
            ToDate = toDateOnly,
            TotalDays = totalDays,
            Status = "Pending",
            Reason = createDto.Reason,
            RequestedAt = DateTime.Now
        };

        _context.HrLeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        return new LeaveRequestDTO
        {
            Id = leaveRequest.Id,
            WorkerId = leaveRequest.WorkerId,
            WorkerName = "",
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = "",
            FromDate = leaveRequest.FromDate.ToDateTime(TimeOnly.MinValue),
            ToDate = leaveRequest.ToDate.ToDateTime(TimeOnly.MinValue),
            TotalDays = leaveRequest.TotalDays,
            Status = leaveRequest.Status ?? "Pending",
            Reason = leaveRequest.Reason,
            RequestedAt = leaveRequest.RequestedAt ?? DateTime.Now
        };
    }

    public async Task<LeaveRequestDTO> ApproveLeaveRequestAsync(ApproveLeaveRequestDTO approveDto)
    {
        var leaveRequest = await _context.HrLeaveRequests.FindAsync(approveDto.Id);
        if (leaveRequest == null)
        {
            throw new ArgumentException("Leave request not found");
        }

        if (leaveRequest.Status != "Pending")
        {
            throw new InvalidOperationException("Leave request is not in pending status");
        }

        leaveRequest.Status = approveDto.Status;
        leaveRequest.ApprovedBy = approveDto.ApprovedBy;
        leaveRequest.ApprovedAt = DateTime.Now;
        leaveRequest.RejectionReason = approveDto.RejectionReason;

        // If approved, update leave balance
        if (approveDto.Status == "Approved")
        {
            var currentYear = leaveRequest.FromDate.Year;
            var leaveBalance = await _context.HrLeaveBalances
                .FirstOrDefaultAsync(lb => lb.WorkerId == leaveRequest.WorkerId && 
                                         lb.LeaveTypeId == leaveRequest.LeaveTypeId && 
                                         lb.Year == (short)currentYear);

            if (leaveBalance != null)
            {
                leaveBalance.Used += leaveRequest.TotalDays;
                leaveBalance.Balance = leaveBalance.Allocated - leaveBalance.Used;
            }
        }

        await _context.SaveChangesAsync();

        return new LeaveRequestDTO
        {
            Id = leaveRequest.Id,
            WorkerId = leaveRequest.WorkerId,
            WorkerName = "",
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = "",
            FromDate = leaveRequest.FromDate.ToDateTime(TimeOnly.MinValue),
            ToDate = leaveRequest.ToDate.ToDateTime(TimeOnly.MinValue),
            TotalDays = leaveRequest.TotalDays,
            Status = leaveRequest.Status ?? "Pending",
            Reason = leaveRequest.Reason,
            RejectionReason = leaveRequest.RejectionReason,
            RequestedAt = leaveRequest.RequestedAt ?? DateTime.Now,
            ApprovedBy = leaveRequest.ApprovedBy,
            ApprovedAt = leaveRequest.ApprovedAt
        };
    }

    public async Task<bool> CancelLeaveRequestAsync(int id, int cancelledBy)
    {
        var leaveRequest = await _context.HrLeaveRequests.FindAsync(id);
        if (leaveRequest == null) return false;

        if (leaveRequest.Status == "Approved")
        {
            // Restore leave balance if it was approved
            var currentYear = leaveRequest.FromDate.Year;
            var leaveBalance = await _context.HrLeaveBalances
                .FirstOrDefaultAsync(lb => lb.WorkerId == leaveRequest.WorkerId && 
                                         lb.LeaveTypeId == leaveRequest.LeaveTypeId && 
                                         lb.Year == (short)currentYear);

            if (leaveBalance != null)
            {
                leaveBalance.Used -= leaveRequest.TotalDays;
                leaveBalance.Balance = leaveBalance.Allocated - leaveBalance.Used;
            }
        }

        leaveRequest.Status = "Cancelled";
        leaveRequest.ApprovedBy = cancelledBy;
        leaveRequest.ApprovedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    // Leave Balance
    public async Task<IEnumerable<LeaveBalanceDTO>> GetLeaveBalanceAsync(int workerId, int? year = null)
    {
        var currentYear = (short)(year ?? DateTime.Now.Year);

        var leaveBalances = await _context.HrLeaveBalances
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.WorkerId == workerId && lb.Year == currentYear)
            .ToListAsync();

        return leaveBalances.Select(lb => new LeaveBalanceDTO
        {
            Id = lb.Id,
            WorkerId = lb.WorkerId,
            WorkerName = "", // Will be populated from worker table join if needed
            LeaveTypeId = lb.LeaveTypeId,
            LeaveTypeName = lb.LeaveType?.Name ?? "",
            LeaveTypeCode = lb.LeaveType?.Code ?? "",
            Allocated = lb.Allocated ?? 0,
            Used = lb.Used ?? 0,
            Balance = (lb.Allocated ?? 0) - (lb.Used ?? 0), // Calculate remaining balance
            Year = lb.Year
        });
    }

    public async Task<LeaveBalanceDTO> AllocateLeaveBalanceAsync(AllocateLeaveBalanceDTO allocateDto)
    {
        var existingBalance = await _context.HrLeaveBalances
            .FirstOrDefaultAsync(lb => lb.WorkerId == allocateDto.WorkerId && 
                               lb.LeaveTypeId == allocateDto.LeaveTypeId && 
                               lb.Year == (short)allocateDto.Year);

        if (existingBalance != null)
        {
            // Update existing allocation
            existingBalance.Allocated = allocateDto.Allocated;
            existingBalance.Balance = existingBalance.Allocated - (existingBalance.Used ?? 0);
        }
        else
        {
            // Create new allocation
            existingBalance = new HrLeaveBalance
            {
                WorkerId = allocateDto.WorkerId,
                LeaveTypeId = allocateDto.LeaveTypeId,
                Allocated = allocateDto.Allocated,
                Used = 0,
                Balance = allocateDto.Allocated,
                Year = (short)allocateDto.Year
            };
            _context.HrLeaveBalances.Add(existingBalance);
        }

        await _context.SaveChangesAsync();

        return new LeaveBalanceDTO
        {
            Id = existingBalance.Id,
            WorkerId = existingBalance.WorkerId,
            WorkerName = "",
            LeaveTypeId = existingBalance.LeaveTypeId,
            LeaveTypeName = "",
            LeaveTypeCode = "",
            Allocated = existingBalance.Allocated ?? 0,
            Used = existingBalance.Used ?? 0,
            Balance = existingBalance.Balance ?? 0,
            Year = existingBalance.Year
        };
    }

    public async Task<bool> AutoAllocateLeaveBalanceAsync(int workerId, int year)
    {
        var leaveTypes = await _context.HrMasterLeaveTypes
            .Where(lt => lt.IsActive == true && lt.MaxDaysPerYear.HasValue)
            .ToListAsync();

        foreach (var leaveType in leaveTypes)
        {
            var existingBalance = await _context.HrLeaveBalances
                .FirstOrDefaultAsync(lb => lb.WorkerId == workerId && 
                                   lb.LeaveTypeId == leaveType.Id && 
                                   lb.Year == (short)year);

            if (existingBalance == null)
            {
                var leaveBalance = new HrLeaveBalance
                {
                    WorkerId = workerId,
                    LeaveTypeId = leaveType.Id,
                    Allocated = leaveType.MaxDaysPerYear ?? 0,
                    Used = 0,
                    Balance = leaveType.MaxDaysPerYear ?? 0,
                    Year = (short)year
                };
                _context.HrLeaveBalances.Add(leaveBalance);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // Validation
    public async Task<bool> HasSufficientBalanceAsync(int workerId, int leaveTypeId, decimal days, int year)
    {
        var leaveBalance = await _context.HrLeaveBalances
            .FirstOrDefaultAsync(lb => lb.WorkerId == workerId && 
                               lb.LeaveTypeId == leaveTypeId && 
                               lb.Year == (short)year);

        return leaveBalance != null && (leaveBalance.Balance ?? 0) >= days;
    }

    public async Task<bool> HasOverlappingLeaveAsync(int workerId, DateTime fromDate, DateTime toDate)
    {
        var fromDateOnly = DateOnly.FromDateTime(fromDate);
        var toDateOnly = DateOnly.FromDateTime(toDate);
        
        var overlapping = await _context.HrLeaveRequests
            .AnyAsync(lr => lr.WorkerId == workerId && 
                          lr.Status == "Approved" &&
                          ((lr.FromDate <= fromDateOnly && lr.ToDate >= fromDateOnly) ||
                           (lr.FromDate <= toDateOnly && lr.ToDate >= toDateOnly) ||
                           (lr.FromDate >= fromDateOnly && lr.ToDate <= toDateOnly)));

        return overlapping;
    }

    public async Task<decimal> CalculateLeaveDaysAsync(DateTime fromDate, DateTime toDate)
    {
        // Simple calculation - can be enhanced to exclude weekends/holidays
        var totalDays = (toDate - fromDate).Days + 1;
        
        // Get working days configuration
        var workingDays = await _context.HrMasterCalendarConfigs
            .Where(cc => cc.IsWorkingDay == true)
            .Select(cc => cc.DayOfWeek)
            .ToListAsync();

        if (!workingDays.Any())
        {
            // If no configuration, assume all days are working days
            return totalDays;
        }

        // Count only working days
        decimal workingDaysCount = 0;
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            if (workingDays.Contains(date.DayOfWeek.ToString()))
            {
                // Check if it's not a holiday
                var dateOnly = DateOnly.FromDateTime(date);
                var isHoliday = await _context.HrMasterHolidays
                    .AnyAsync(h => h.HolidayDate == dateOnly && h.IsActive == true);
                
                if (!isHoliday)
                {
                    workingDaysCount++;
                }
            }
        }

        return workingDaysCount;
    }
} 