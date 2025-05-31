using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.HR;

public partial class HrAttendance
{
    public int Id { get; set; }

    public int WorkerId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public string? Shift { get; set; }

    public string? Status { get; set; }

    public int? LeaveTypeId { get; set; }

    public string? Remarks { get; set; }

    public int MarkedBy { get; set; }

    public DateTime? MarkedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public TimeOnly? CheckInTime { get; set; }

    public TimeOnly? CheckOutTime { get; set; }

    public string? HalfDayType { get; set; }

    public virtual HrMasterLeaveType? LeaveType { get; set; }
}
