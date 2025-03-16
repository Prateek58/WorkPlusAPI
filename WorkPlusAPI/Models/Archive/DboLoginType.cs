using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class DboLoginType
{
    public sbyte LoginTypeId { get; set; }

    public string LoginTypeName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<DboUser> DboUsers { get; set; } = new List<DboUser>();
}
