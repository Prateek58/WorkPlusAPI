using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkPlusAPI.WorkPlus.Data.UserSettings;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Model.UserSettings;

namespace WorkPlusAPI.WorkPlus.Service;

public class UserSettingsService : IUserSettingsService
{
    private readonly UserSettingsContext _context;
    private readonly ILogger<UserSettingsService> _logger;

    public UserSettingsService(UserSettingsContext context, ILogger<UserSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UserSettingDTO>> GetUserSettingsAsync(int userId)
    {
        try
        {
            var settings = await _context.UserSettings
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.SettingKey)
                .ToListAsync();

            return settings.Select(MapToDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user settings for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSettingDTO?> GetUserSettingAsync(int userId, string settingKey)
    {
        try
        {
            var setting = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == settingKey);

            return setting != null ? MapToDTO(setting) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user setting {SettingKey} for user {UserId}", settingKey, userId);
            throw;
        }
    }

    public async Task<UserSettingDTO> CreateOrUpdateSettingAsync(int userId, CreateUserSettingDTO createDto)
    {
        try
        {
            var existingSetting = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == createDto.SettingKey);

            if (existingSetting != null)
            {
                // Update existing setting
                existingSetting.SettingValue = createDto.SettingValue;
                existingSetting.SettingType = createDto.SettingType;
                existingSetting.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Create new setting
                existingSetting = new UserSetting
                {
                    UserId = userId,
                    SettingKey = createDto.SettingKey,
                    SettingValue = createDto.SettingValue,
                    SettingType = createDto.SettingType,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.UserSettings.Add(existingSetting);
            }

            await _context.SaveChangesAsync();
            return MapToDTO(existingSetting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating user setting {SettingKey} for user {UserId}", createDto.SettingKey, userId);
            throw;
        }
    }

    public async Task<bool> DeleteSettingAsync(int userId, string settingKey)
    {
        try
        {
            var setting = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == settingKey);

            if (setting == null)
                return false;

            _context.UserSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user setting {SettingKey} for user {UserId}", settingKey, userId);
            throw;
        }
    }

    // Theme-specific convenience methods
    public async Task<string> GetThemeModeAsync(int userId)
    {
        try
        {
            var setting = await GetUserSettingAsync(userId, "theme_mode");
            return setting?.SettingValue ?? "light"; // Default to light theme
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme mode for user {UserId}", userId);
            return "light";
        }
    }

    public async Task<UserSettingDTO> SetThemeModeAsync(int userId, string mode)
    {
        return await CreateOrUpdateSettingAsync(userId, new CreateUserSettingDTO
        {
            SettingKey = "theme_mode",
            SettingValue = mode,
            SettingType = "string"
        });
    }

    public async Task<object?> GetThemeColorsAsync(int userId)
    {
        try
        {
            var setting = await GetUserSettingAsync(userId, "theme_colors");
            if (setting?.SettingValue != null)
            {
                return JsonSerializer.Deserialize<object>(setting.SettingValue);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme colors for user {UserId}", userId);
            return null;
        }
    }

    public async Task<UserSettingDTO> SetThemeColorsAsync(int userId, object colors)
    {
        var jsonValue = JsonSerializer.Serialize(colors);
        return await CreateOrUpdateSettingAsync(userId, new CreateUserSettingDTO
        {
            SettingKey = "theme_colors",
            SettingValue = jsonValue,
            SettingType = "json"
        });
    }

    public async Task<IEnumerable<UserSettingDTO>> UpdateMultipleSettingsAsync(int userId, IEnumerable<CreateUserSettingDTO> settings)
    {
        var results = new List<UserSettingDTO>();
        
        foreach (var setting in settings)
        {
            var result = await CreateOrUpdateSettingAsync(userId, setting);
            results.Add(result);
        }

        return results;
    }

    private static UserSettingDTO MapToDTO(UserSetting setting)
    {
        return new UserSettingDTO
        {
            Id = setting.Id,
            UserId = setting.UserId,
            SettingKey = setting.SettingKey,
            SettingValue = setting.SettingValue,
            SettingType = setting.SettingType,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt
        };
    }
} 