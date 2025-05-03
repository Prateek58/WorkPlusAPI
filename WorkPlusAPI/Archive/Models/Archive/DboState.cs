using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Archive.Models.Archive;

public partial class DboState
{
    public int StateId { get; set; }

    public string StateName { get; set; } = null!;

    public string? CountryName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DboCity> DboCities { get; set; } = new List<DboCity>();
}
