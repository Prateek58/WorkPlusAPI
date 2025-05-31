using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.HR;

public partial class HrLeaveRequest
{
    public int Id { get; set; }

    public int WorkerId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public decimal TotalDays { get; set; }

    public string? Status { get; set; }

    public string? Reason { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? RequestedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual HrMasterLeaveType LeaveType { get; set; } = null!;
}
