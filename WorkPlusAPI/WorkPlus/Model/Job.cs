using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model;

public partial class Job
{
    public int JobId { get; set; }

    public string JobName { get; set; } = null!;

    public int JobTypeId { get; set; }

    public decimal? RatePerItem { get; set; }

    public decimal? RatePerHour { get; set; }

    public decimal? ExpectedHours { get; set; }

    public int? ExpectedItemsPerHour { get; set; }

    public decimal? IncentiveBonusRate { get; set; }

    public decimal? PenaltyRate { get; set; }

    public string? IncentiveType { get; set; }

    public int CreatedBy { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<JobEntry> JobEntries { get; set; } = new List<JobEntry>();

    public virtual JobType JobType { get; set; } = null!;
}
