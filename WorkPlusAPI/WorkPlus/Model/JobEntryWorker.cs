using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model;

public partial class JobEntryWorker
{
    public int Id { get; set; }

    public int EntryId { get; set; }

    public int WorkerId { get; set; }

    public virtual JobEntry Entry { get; set; } = null!;

    public virtual Worker Worker { get; set; } = null!;
}
