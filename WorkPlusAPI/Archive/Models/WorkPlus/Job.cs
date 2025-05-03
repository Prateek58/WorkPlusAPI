using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkPlusAPI.Archive.Models.WorkPlus;

[Table("jobs")]
public class Job
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [Column("is_group")]
    public bool IsGroup { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation property for parent job
    public virtual Job? Parent { get; set; }

    // Navigation property for child jobs
    public virtual ICollection<Job> Children { get; set; } = new List<Job>();
}