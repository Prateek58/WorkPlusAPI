using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.LR;

public partial class LrDocument
{
    public int DocumentId { get; set; }

    public int LrEntryId { get; set; }

    public int TypeId { get; set; }

    public string DocumentName { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual LrEntry LrEntry { get; set; } = null!;

    public virtual LrDocumentType Type { get; set; } = null!;
}
