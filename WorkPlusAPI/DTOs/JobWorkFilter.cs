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
} 