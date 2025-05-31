using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model.UserSettings;

public partial class UserSetting
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string? SettingType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
