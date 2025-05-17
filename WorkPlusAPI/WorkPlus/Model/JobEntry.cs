using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model;

public partial class JobEntry
{
    public int EntryId { get; set; }

    public int JobId { get; set; }

    public string EntryType { get; set; } = null!;

    public int? WorkerId { get; set; }

    public int? GroupId { get; set; }

    public bool IsPostLunch { get; set; }

    public int? ItemsCompleted { get; set; }

    public decimal? HoursTaken { get; set; }

    public decimal RatePerJob { get; set; }

    public decimal? ExpectedHours { get; set; }

    public decimal? ProductiveHours { get; set; }

    public decimal? ExtraHours { get; set; }

    public decimal? UnderperformanceHours { get; set; }

    public decimal? IncentiveAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Remarks { get; set; }

    public bool? IsFinalized { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual JobGroup? Group { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual ICollection<JobEntryWorker> JobEntryWorkers { get; set; } = new List<JobEntryWorker>();

    public virtual Worker? Worker { get; set; }
}
