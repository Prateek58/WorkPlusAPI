using System.ComponentModel.DataAnnotations;

namespace WorkPlusAPI.Archive.Models.Auth;

public class UpdatePasswordRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = string.Empty;
} 