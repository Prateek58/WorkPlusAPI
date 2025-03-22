namespace WorkPlusAPI.DTOs;

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

public class JobWorkFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? JobId { get; set; }
    public string? JobWorkTypeId { get; set; }
    public string? UnitId { get; set; }
    public string? EmployeeId { get; set; }
}

public class JobWorkDto
{
    public string Id { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string JobWorkTypeId { get; set; } = string.Empty;
    public string JobWorkTypeName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public decimal Quantity { get; set; }
    public string UnitId { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
}

public class JobWorkSummaryDto
{
    public decimal TotalHours { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalRecords { get; set; }
} 