using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.LR;

public partial class Statecity
{
    public int CityId { get; set; }

    public string CityName { get; set; } = null!;

    public string Latitude { get; set; } = null!;

    public string Longitude { get; set; } = null!;

    public string State { get; set; } = null!;

    public virtual ICollection<LrEntry> LrEntryDestinationCities { get; set; } = new List<LrEntry>();

    public virtual ICollection<LrEntry> LrEntryOriginCities { get; set; } = new List<LrEntry>();

    public virtual ICollection<LrParty> LrParties { get; set; } = new List<LrParty>();

    public virtual ICollection<LrTransporter> LrTransporters { get; set; } = new List<LrTransporter>();

    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();
}
