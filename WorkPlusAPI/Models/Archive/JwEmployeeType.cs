using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class JwEmployeeType
{
    public sbyte EmployeeTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<JwEmployee> JwEmployees { get; set; } = new List<JwEmployee>();
}
