using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Archive.Models.Archive;

public partial class LrEntry
{
    public long EntryId { get; set; }

    public sbyte? UnitId { get; set; }

    public long? PartyId { get; set; }

    public int? CityId { get; set; }

    public string? CityName { get; set; }

    public string? BillNo { get; set; }

    public DateTime? BillDate { get; set; }

    public string? BillAmount { get; set; }

    public int? TransporterId { get; set; }

    public string? LrNo { get; set; }

    public DateTime? LrDate { get; set; }

    public decimal? LrAmount { get; set; }

    public decimal? Freight { get; set; }

    public decimal? OtherExpenses { get; set; }

    public decimal? TotalFreight { get; set; }

    public decimal? LrWeight { get; set; }

    public decimal? RatePerQtl { get; set; }

    public string? TruckNo { get; set; }

    public decimal? LrQty { get; set; }

    public decimal? Wces { get; set; }

    public decimal? LooseEs { get; set; }

    public decimal? FMills { get; set; }

    public decimal? Hopper { get; set; }

    public decimal? Spares { get; set; }

    public decimal? Others { get; set; }

    public decimal? TotalQty { get; set; }

    public DateTime? ReceivedOn { get; set; }

    public string? ReceivedBy { get; set; }

    public string? ReceivedNotes { get; set; }

    public bool? IsEmailSent { get; set; }

    public DateTime? CreatedAt { get; set; }
}
