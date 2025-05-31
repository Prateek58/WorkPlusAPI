using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.HR;

public partial class HrLeaveBalance
{
    public int Id { get; set; }

    public int WorkerId { get; set; }

    public int LeaveTypeId { get; set; }

    public decimal? Allocated { get; set; }

    public decimal? Used { get; set; }

    public decimal? Balance { get; set; }

    public short Year { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual HrMasterLeaveType LeaveType { get; set; } = null!;
}
