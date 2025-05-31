using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.HR;

public partial class HrMasterHoliday
{
    public int Id { get; set; }

    public DateOnly HolidayDate { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsOptional { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
