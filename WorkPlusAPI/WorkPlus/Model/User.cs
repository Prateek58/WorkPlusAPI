using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<JobEntry> JobEntries { get; set; } = new List<JobEntry>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual Worker? Worker { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
