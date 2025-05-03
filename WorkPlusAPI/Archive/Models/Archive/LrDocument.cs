using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Archive.Models.Archive;

public partial class LrDocument
{
    public long DocumentId { get; set; }

    public long? LrEntryId { get; set; }

    public string? FileName { get; set; }

    public DateTime? CreatedAt { get; set; }
}
