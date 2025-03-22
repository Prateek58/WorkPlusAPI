namespace WorkPlusAPI.Models.JobWork;

public class JobWorkFilterDto
{
    public int? UnitId { get; set; }
    public int? JobWorkTypeId { get; set; }
    public bool? IsJobWorkGroup { get; set; }
    public int? JobId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
}

public enum ApprovalStatus
{
    All,
    Approved,
    NotApproved
} 