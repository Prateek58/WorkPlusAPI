using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class DboCity
{
    public int CityId { get; set; }

    public string CityName { get; set; } = null!;

    public int? StateId { get; set; }

    public string? PinCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? StateName { get; set; }

    public string? CountryName { get; set; }

    public virtual DboState? State { get; set; }
}
