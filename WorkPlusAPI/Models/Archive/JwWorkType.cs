using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class JwWorkType
{
    public sbyte WorkTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
