using System;

namespace WorkPlusAPI.WorkPlus.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? Password { get; set; } // For creating new users or changing passwords
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 