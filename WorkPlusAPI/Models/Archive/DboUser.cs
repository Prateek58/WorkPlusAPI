using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Models.Archive;

public partial class DboUser
{
    public short UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool? IsEnabled { get; set; }

    public bool? IsAdmin { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public sbyte? LoginTypeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual DboLoginType? LoginType { get; set; }
}
