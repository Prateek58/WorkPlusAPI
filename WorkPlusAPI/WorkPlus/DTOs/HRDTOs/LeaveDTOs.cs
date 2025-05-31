using System;

namespace WorkPlusAPI.WorkPlus.DTOs.HRDTOs;

public class LeaveTypeDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public string AppliesTo { get; set; } = "All";
    public int? MaxDaysPerYear { get; set; }
    public bool IsActive { get; set; }
}

public class LeaveBalanceDTO
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public decimal Allocated { get; set; }
    public decimal Used { get; set; }
    public decimal Balance { get; set; }
    public int Year { get; set; }
}

public class LeaveRequestDTO
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime RequestedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class CreateLeaveRequestDTO
{
    public int WorkerId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? Reason { get; set; }
}

public class ApproveLeaveRequestDTO
{
    public int Id { get; set; }
    public string Status { get; set; } = "Approved"; // Approved, Rejected
    public string? RejectionReason { get; set; }
    public int ApprovedBy { get; set; }
}

public class AllocateLeaveBalanceDTO
{
    public int WorkerId { get; set; }
    public int LeaveTypeId { get; set; }
    public decimal Allocated { get; set; }
    public int Year { get; set; }
} 