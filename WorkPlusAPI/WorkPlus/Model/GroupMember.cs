using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model;

public partial class GroupMember
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int WorkerId { get; set; }

    public virtual JobGroup Group { get; set; } = null!;

    public virtual Worker Worker { get; set; } = null!;
}
