namespace WorkPlusAPI.DTOs;

// Interface for JobWorkDto to ensure type safety
public interface IJobWorkDto
{
    long EntryId { get; set; }
    DateTime? EntryDate { get; set; }
    string? JwNo { get; set; }
    string? WorkName { get; set; }
    string? EmployeeName { get; set; }
    string? UnitName { get; set; }
    string? WorkType { get; set; }
    string? GroupName { get; set; }
    decimal? QtyItems { get; set; }
    decimal? QtyHours { get; set; }
    decimal? RateForJob { get; set; }
    decimal? TotalAmount { get; set; }
    bool? IsApproved { get; set; }
    string? ApprovedBy { get; set; }
    DateTime? ApprovedOn { get; set; }
    string? EntryByUserId { get; set; }
    string? WorkId { get; set; }
    string? Remarks { get; set; }
}

public class UnitDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class JobWorkTypeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class JobDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGroup { get; set; }
    public string? ParentId { get; set; }
}

public class EmployeeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
}

public class JobWorkFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? JobId { get; set; }
    public string? JobWorkTypeId { get; set; }
    public string? UnitId { get; set; }
    public string? EmployeeId { get; set; }
    public string? JobType { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

public class JobWorkDto : IJobWorkDto
{
    public long EntryId { get; set; }
    public DateTime? EntryDate { get; set; }
    public string? JwNo { get; set; }
    public string? WorkName { get; set; }
    public string? EmployeeName { get; set; }
    public string? UnitName { get; set; }
    public string? WorkType { get; set; }
    public string? GroupName { get; set; }
    public decimal? QtyItems { get; set; }
    public decimal? QtyHours { get; set; }
    public decimal? RateForJob { get; set; }
    public decimal? TotalAmount { get; set; }
    public bool? IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedOn { get; set; }
    public string? EntryByUserId { get; set; }
    public string? WorkId { get; set; }
    public string? Remarks { get; set; }
}

public class JobWorkSummaryDto
{
    public decimal TotalHours { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalRecords { get; set; }
}

public class JobWorkResponse
{
    public List<IJobWorkDto> Data { get; set; } = new List<IJobWorkDto>();
    public int Total { get; set; }
} 