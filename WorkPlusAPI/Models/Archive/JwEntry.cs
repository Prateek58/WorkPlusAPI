using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class JwEntry
{
    public long EntryId { get; set; }

    public sbyte? UnitId { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? JwNo { get; set; }

    public int? EmployeeId { get; set; }

    public int? WorkId { get; set; }

    public decimal? QtyItems { get; set; }

    public decimal? QtyHours { get; set; }

    public decimal? RateForJob { get; set; }

    public decimal? TotalAmount { get; set; }

    public short? EntryByUserId { get; set; }

    public bool? IsApproved { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public DateTime? CreatedAt { get; set; }
}
