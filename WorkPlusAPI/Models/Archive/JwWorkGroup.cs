using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class JwWorkGroup
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
