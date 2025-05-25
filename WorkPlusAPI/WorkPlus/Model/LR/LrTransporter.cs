using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.LR;

public partial class LrTransporter
{
    public int TransporterId { get; set; }

    public string TransporterName { get; set; } = null!;

    public string? TransporterCode { get; set; }

    public string? ContactPerson { get; set; }

    public string? Address { get; set; }

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
