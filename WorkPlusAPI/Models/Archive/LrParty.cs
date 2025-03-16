using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class LrParty
{
    public long PartyId { get; set; }

    public string PartyName { get; set; } = null!;

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public int? CityId { get; set; }

    public string? PinCode { get; set; }

    public string? MobilePhone { get; set; }

    public string? Telephone { get; set; }

    public string? CityName { get; set; }

    public string? LoginName { get; set; }

    public string? LoginPassword { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }
}
