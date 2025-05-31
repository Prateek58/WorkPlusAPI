using WorkPlusAPI.WorkPlus.DTOs;

namespace WorkPlusAPI.WorkPlus.Service;

public interface IUserSettingsService
{
    // General settings operations
    Task<IEnumerable<UserSettingDTO>> GetUserSettingsAsync(int userId);
    Task<UserSettingDTO?> GetUserSettingAsync(int userId, string settingKey);
    Task<UserSettingDTO> CreateOrUpdateSettingAsync(int userId, CreateUserSettingDTO createDto);
    Task<bool> DeleteSettingAsync(int userId, string settingKey);
    
    // Theme-specific convenience methods
    Task<string> GetThemeModeAsync(int userId);
    Task<UserSettingDTO> SetThemeModeAsync(int userId, string mode);
    Task<object?> GetThemeColorsAsync(int userId);
    Task<UserSettingDTO> SetThemeColorsAsync(int userId, object colors);
    
    // Bulk operations
    Task<IEnumerable<UserSettingDTO>> UpdateMultipleSettingsAsync(int userId, IEnumerable<CreateUserSettingDTO> settings);
} 