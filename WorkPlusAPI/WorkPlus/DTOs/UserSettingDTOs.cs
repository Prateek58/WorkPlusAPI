namespace WorkPlusAPI.WorkPlus.DTOs;

public class UserSettingDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public string? SettingType { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserSettingDTO
{
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public string SettingType { get; set; } = "string";
}

public class UpdateUserSettingDTO
{
    public string? SettingValue { get; set; }
    public string? SettingType { get; set; }
}

public class BulkUserSettingsDTO
{
    public IEnumerable<CreateUserSettingDTO> Settings { get; set; } = new List<CreateUserSettingDTO>();
} 