using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkPlusAPI.Models.WorkPlus;

[Table("jobworks")]
public class JobWork
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("unit_id")]
    public int UnitId { get; set; }

    [Column("job_work_type_id")]
    public int JobWorkTypeId { get; set; }

    [Column("job_id")]
    public int JobId { get; set; }

    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("total_hours")]
    public decimal TotalHours { get; set; }

    [Column("rate_per_day")]
    public decimal RatePerDay { get; set; }

    [Column("rate_per_pc")]
    public decimal RatePerPc { get; set; }

    [Column("total_qty")]
    public decimal TotalQty { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("is_approved")]
    public bool IsApproved { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Unit Unit { get; set; } = null!;
    public virtual JobWorkType JobWorkType { get; set; } = null!;
    public virtual Job Job { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
} 