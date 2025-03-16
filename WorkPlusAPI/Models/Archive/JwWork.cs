using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class JwWork
{
    public int WorkId { get; set; }

    public string WorkName { get; set; } = null!;

    public sbyte? WorkTypeId { get; set; }

    public sbyte? TimePerPiece { get; set; }

    public decimal? RatePerPiece { get; set; }

    public int? GroupId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
