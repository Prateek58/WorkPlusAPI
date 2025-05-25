using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.LR;

public partial class LrEntry
{
    public int EntryId { get; set; }

    public int UnitId { get; set; }

    public int PartyId { get; set; }

    public int TransporterId { get; set; }

    public string LrNo { get; set; } = null!;

    public DateOnly LrDate { get; set; }

    public DateOnly? BillDate { get; set; }

    public string? BillNo { get; set; }

    public string? TruckNo { get; set; }

    public decimal? LrWeight { get; set; }

    public decimal? RatePerQtl { get; set; }

    public decimal? LrQty { get; set; }

    public decimal? LrAmount { get; set; }

    public decimal? Freight { get; set; }

    public decimal? OtherExpenses { get; set; }

    public decimal? TotalFreight { get; set; }

    public decimal? TotalQty { get; set; }

    public decimal? BillAmount { get; set; }

    public int? OriginCityId { get; set; }

    public int? DestinationCityId { get; set; }

    public string? DriverName { get; set; }

    public string? DriverMobile { get; set; }

    public string? Remarks { get; set; }

    public string? Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Statecity? DestinationCity { get; set; }

    public virtual ICollection<LrDocument> LrDocuments { get; set; } = new List<LrDocument>();

    public virtual Statecity? OriginCity { get; set; }

    public virtual LrParty Party { get; set; } = null!;

    public virtual LrTransporter Transporter { get; set; } = null!;

    public virtual Unit Unit { get; set; } = null!;
}
