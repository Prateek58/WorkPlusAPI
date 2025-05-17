using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model;

public partial class JobGroup
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public int MinWorkers { get; set; }

    public int MaxWorkers { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<JobEntry> JobEntries { get; set; } = new List<JobEntry>();
}
