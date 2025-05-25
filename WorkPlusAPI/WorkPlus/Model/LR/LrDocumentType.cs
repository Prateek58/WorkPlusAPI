using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.LR;

public partial class LrDocumentType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string AllowedExtensions { get; set; } = null!;

    public virtual ICollection<LrDocument> LrDocuments { get; set; } = new List<LrDocument>();
}
