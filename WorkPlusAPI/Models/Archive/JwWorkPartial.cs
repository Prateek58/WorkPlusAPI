using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class JwWork
{
    public sbyte? UnitId { get; set; }
    public int? EmployeeId { get; set; }

    // Navigation properties
    public virtual JwWorkGroup? WorkGroup { get; set; }
    public virtual JwWorkType? WorkType { get; set; }
    public virtual JwEmployee? Employee { get; set; }
    public virtual DboUnit? Unit { get; set; }
} 