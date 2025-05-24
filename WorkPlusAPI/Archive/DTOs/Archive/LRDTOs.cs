namespace WorkPlusAPI.Archive.DTOs.Archive;

// Main LR Entry DTO
public class LREntryDto
{
    public long EntryId { get; set; }
    public byte? UnitId { get; set; }
    public string? UnitName { get; set; }
    public long? PartyId { get; set; }
    public string? PartyName { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public string? BillAmount { get; set; }
    public int? TransporterId { get; set; }
    public string? TransporterName { get; set; }
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

// LR Filter DTO
public class LRFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public byte? UnitId { get; set; }
    public long? PartyId { get; set; }
    public int? TransporterId { get; set; }
    public int? CityId { get; set; }
    public string? BillNo { get; set; }
    public string? LrNo { get; set; }
    public string? TruckNo { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public string? Columns { get; set; }
}

// LR Response DTO
public class LRResponse
{
    public List<LREntryDto> Data { get; set; } = new List<LREntryDto>();
    public int Total { get; set; }
}

// LR Summary DTO
public class LRSummaryDto
{
    public int TotalEntries { get; set; }
    public int TotalParties { get; set; }
    public int TotalTransporters { get; set; }
    public int TotalCities { get; set; }
    public decimal TotalLrAmount { get; set; }
    public decimal TotalFreight { get; set; }
    public decimal TotalOtherExpenses { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalQuantity { get; set; }
    public int TotalRecords { get; set; }
    public List<UnitSummary> UnitSummaries { get; set; } = new List<UnitSummary>();
}

// Unit Summary for LR
public class UnitSummary
{
    public byte UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int EntryCount { get; set; }
    public decimal TotalLrAmount { get; set; }
    public decimal TotalFreight { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalQuantity { get; set; }
}

// LR Party DTO
public class LRPartyDto
{
    public long PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public string? PinCode { get; set; }
    public string? MobilePhone { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
}

// LR Transporter DTO
public class LRTransporterDto
{
    public int TransporterId { get; set; }
    public string TransporterName { get; set; } = string.Empty;
}

// LR Document DTO
public class LRDocumentDto
{
    public long DocumentId { get; set; }
    public long? LrEntryId { get; set; }
    public string? FileName { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// City DTO for LR
public class LRCityDto
{
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
}

// Unit DTO for LR
public class LRUnitDto
{
    public byte UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
}
