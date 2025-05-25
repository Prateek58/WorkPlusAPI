using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.LR;

public partial class LrParty
{
    public int PartyId { get; set; }

    public string PartyName { get; set; } = null!;

    public string? PartyCode { get; set; }

    public string? ContactPerson { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public int? CityId { get; set; }

    public string? Pincode { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Email { get; set; }

    public string? Gstin { get; set; }

    public string? Pan { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Statecity? City { get; set; }

    public virtual ICollection<LrEntry> LrEntries { get; set; } = new List<LrEntry>();
}
