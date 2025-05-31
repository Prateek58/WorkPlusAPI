using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.HR;

public partial class HrMasterCalendarConfig
{
    public int Id { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public bool? IsWorkingDay { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
