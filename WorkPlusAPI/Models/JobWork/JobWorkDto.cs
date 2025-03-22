namespace WorkPlusAPI.Models.JobWork;

public class JobWorkDto
{
    public int Id { get; set; }
    public string UnitName { get; set; } = null!;
    public DateTime Date { get; set; }
    public string JobWorkTypeName { get; set; } = null!;
    public string JobName { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public decimal TotalHours { get; set; }
    public decimal RatePerDay { get; set; }
    public decimal RatePerPc { get; set; }
    public decimal TotalQty { get; set; }
    public decimal Amount { get; set; }
    public bool IsApproved { get; set; }
}

public class JobWorkSummaryDto
{
    public decimal TotalHours { get; set; }
    public decimal TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalRecords { get; set; }
} 