using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Archive.Models.Archive;

public partial class LrTransporter
{
    public int TransporterId { get; set; }

    public string TransporterName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
