using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.HR;

public partial class HrMasterLeaveType
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool? IsPaid { get; set; }

    public string? AppliesTo { get; set; }

    public int? MaxDaysPerYear { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<HrAttendance> HrAttendances { get; set; } = new List<HrAttendance>();

    public virtual ICollection<HrLeaveBalance> HrLeaveBalances { get; set; } = new List<HrLeaveBalance>();

    public virtual ICollection<HrLeaveRequest> HrLeaveRequests { get; set; } = new List<HrLeaveRequest>();
}
