using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.LR;

public partial class Unit
{
    public int UnitId { get; set; }

    public string UnitName { get; set; } = null!;

    public string? UnitCode { get; set; }

    public string? Address { get; set; }

    public int? CityId { get; set; }

    public string? Pincode { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Gstin { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Statecity? City { get; set; }

    public virtual ICollection<LrEntry> LrEntries { get; set; } = new List<LrEntry>();
}
