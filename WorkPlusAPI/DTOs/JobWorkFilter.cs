using System;

namespace WorkPlusAPI.DTOs;

public class JobWorkFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? JobId { get; set; }
    public string? JobWorkTypeId { get; set; }
    public string? UnitId { get; set; }
    public string? EmployeeId { get; set; }
    public string? JobType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
} 