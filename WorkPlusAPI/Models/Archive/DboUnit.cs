using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class DboUnit
{
    public sbyte UnitId { get; set; }

    public string UnitName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
